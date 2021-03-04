using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BusLane.Exceptions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace BusLane.Transport.RabbitMQ.Consuming
{
    /// <summary>
    /// A <see cref="IMessageReceiver"/> that reads transient messages from the rabbitmq message broker.
    /// If an error occurs during message handling, the message will not be re-consumed.
    /// </summary>
    internal sealed class RabbitMqMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// A cancellation token source used to abort operation.
        /// </summary>
        private readonly CancellationTokenSource _Cancellation;

        private readonly ConcurrentDictionary<string, Func<ReadOnlyMemory<byte>, Task>> _SubscriptionCallbacks =
            new ConcurrentDictionary<string, Func<ReadOnlyMemory<byte>, Task>>();

        /// <summary>
        /// Gets the name of the queue being used on the message broker.
        /// </summary>
        private readonly string _QueueName;

        /// <summary>
        /// Gets the logger to write to.
        /// </summary>
        private readonly ILogger _Logger;

        /// <summary>
        /// Gets the connection to the message broker.
        /// </summary>
        private readonly IConnection _Connection;

        /// <summary>
        /// Gets the channel to use when performing actions on the message broker.
        /// </summary>
        private readonly IModel _Channel;

        /// <summary>
        /// Gets the name of the exchange being used on the message broker.
        /// </summary>
        private readonly string _ExchangeName;

        /// <summary>
        /// Marker if disposal of the receiver was triggered
        /// </summary>
        private bool _IsDisposing;

        /// <summary>
        /// Initializes a new <see cref="RabbitMqMessageReceiver"/>
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="connectionFactory">The factory to create connections to the message broker from.</param>
        /// <param name="exchangeName">The name of the exchange to use.</param>
        /// <param name="useDurableExchange">Indicates whether the exchange for durable messages should be used.</param>
        /// <param name="doAutoDeleteExchange">Deletes the exchange when the last channel leaves.</param>
        /// <param name="cancellationToken">A token to cancel operations performed by the consumer with.</param>
        public RabbitMqMessageReceiver(
            ILogger<RabbitMqMessageReceiver> logger,
            IConnectionFactory connectionFactory,
            string exchangeName,
            bool useDurableExchange,
            bool doAutoDeleteExchange,
            CancellationToken cancellationToken)
        {
            _Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _Logger = logger;
            _ExchangeName = exchangeName;
            _Connection = connectionFactory.CreateConnection();
            _Channel = _Connection.CreateModel();

            _Channel.ExchangeDeclare(_ExchangeName, ExchangeType.Topic, useDurableExchange, doAutoDeleteExchange);
            _QueueName = _Channel.QueueDeclare().QueueName;

            EventingBasicConsumer consumer = new EventingBasicConsumer(_Channel);
            consumer.Received += async (_, e) => await OnReceivedMessageAsync(e);
            
            consumer.Registered += (_, e) => _Logger.LogDebug(
                "Registered consumer {ConsumerId}",
                ToLogString(e.ConsumerTags));
            
            consumer.Unregistered += (_, e) => _Logger.LogDebug(
                "Unregistered consumer {ConsumerId}",
                ToLogString(e.ConsumerTags));
            
            consumer.Shutdown += (_, e) => _Logger.LogDebug(
                "Shutdown consumer: {Code} ({Reason})",
                e.ReplyCode,
                e.ReplyText);
            
            consumer.ConsumerCancelled += (_, e) => _Logger.LogDebug(
                "Cancelled consumer {ConsumerId}",
                ToLogString(e.ConsumerTags));
            _Channel.BasicConsume(_QueueName, true, consumer);
        }

        private async Task OnReceivedMessageAsync(BasicDeliverEventArgs eventArguments)
        {
            try
            {
                _Logger.LogDebug("Received message '{DeliveryTag}'", eventArguments.DeliveryTag);
                _Logger.LogTrace(
                    "Received message: body:=byte[{BodyLength}], ConsumerTag:={ConsumerTag}, "
                    + "DeliveryTag:={DeliveryTag}, Exchange:={Exchange}, Redelivered:={Redelivered}, "
                    + "RoutingKey:={RoutingKey}",
                    eventArguments.Body.Length,
                    eventArguments.ConsumerTag,
                    eventArguments.DeliveryTag,
                    eventArguments.Exchange,
                    eventArguments.Redelivered,
                    eventArguments.RoutingKey);

                await HandleMessage(eventArguments.RoutingKey, eventArguments.Body);
            }
            catch (OperationCanceledException)
            {
                _Logger.LogWarning("Cancelled message consumption");
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Error while receiving message '{DeliveryTag}'", eventArguments.DeliveryTag);
            }
        }
        
        private async Task HandleMessage(string topic, ReadOnlyMemory<byte> rawMessage)
        {
            if (_SubscriptionCallbacks.ContainsKey(topic.ToLowerInvariant()))
            {
                await _SubscriptionCallbacks[topic.ToLowerInvariant()].Invoke(rawMessage);
            }
        }

        private static string ToLogString(IEnumerable<string> values)
        {
            return $"[{string.Join(", ", values.Select(value => $"'{value}'"))}]";
        }

        /// <inheritdoc />
        public Task SubscribeAsync(
            string topic,
            Func<ReadOnlyMemory<byte>, Task> messageReceiveAsync,
            CancellationToken cancellationToken = default)
        {
            if (_SubscriptionCallbacks.ContainsKey(topic.ToLowerInvariant()) == false)
            {
                _SubscriptionCallbacks.TryAdd(topic.ToLowerInvariant(), messageReceiveAsync);
                _Channel.QueueBind(_QueueName, _ExchangeName, topic);
                _Logger.LogInformation("Subscribed to topic '{Topic}'", topic);
            }
            return Task.CompletedTask;
        }

        /// <summary>Unsubscribes from a category on the broker.</summary>
        /// <param name="topic">The category to unsubscribe from.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        public Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default)
        {
            _SubscriptionCallbacks.TryRemove(topic.ToLowerInvariant(), out _);
            _Channel.QueueUnbind(_QueueName, _ExchangeName, topic);
            _Logger.LogInformation("Unsubscribed from topic '{Topic}'", topic);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public void Dispose()
        {
            if (_IsDisposing)
            {
                _Logger.LogTrace(
                    "Discarded dispose of the {InstanceName} instance (already triggered)",
                    GetType().FullName);
                return;
            }

            string queueName = _QueueName;

            _Logger.LogTrace(
                "Dispose of the {InstanceName} instance for {QueueName} started",
                GetType().FullName,
                queueName);

            _IsDisposing = true;
            _Cancellation.Cancel();
            _Channel.Close();
            _Connection.Close();
            _Connection.Dispose();
            _Channel.Dispose();
            _Cancellation.Dispose();

            _Logger.LogTrace(
                "Dispose of the {InstanceName} instance for {QueueName} completed",
                GetType().FullName,
                queueName);
        }
    }
}