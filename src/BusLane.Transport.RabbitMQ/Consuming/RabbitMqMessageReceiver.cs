using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IChannel _Channel;

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
        /// <param name="exchangeType">RabbitMQ specific exchange types (direct|fanout|headers|topic)</param>
        /// <param name="queueName">
        /// The name of the queue.
        /// If the same name of the queue is used, a competing consumer pattern is used.
        /// See: https://www.enterpriseintegrationpatterns.com/patterns/messaging/CompetingConsumers.html
        /// and https://www.rabbitmq.com/tutorials/tutorial-two-dotnet.html
        /// </param>
        /// <param name="useDurableQueue">The queue will survive a broker restart</param>
        /// <param name="useExclusiveQueue">Used by only one connection and the queue will be deleted when that connection closes</param>
        /// <param name="autoDeleteQueue">Queue that has had at least one consumer is deleted when last consumer unsubscribes</param>
        /// <param name="cancellationToken">A token to cancel operations performed by the consumer with.</param>
        public RabbitMqMessageReceiver(
            ILogger<RabbitMqMessageReceiver> logger,
            IConnectionFactory connectionFactory,
            string exchangeName,
            bool useDurableExchange,
            bool doAutoDeleteExchange,
            string exchangeType = ExchangeType.Topic,
            string queueName = "",
            bool useDurableQueue = true,
            bool useExclusiveQueue = false,
            bool autoDeleteQueue = false,
            CancellationToken cancellationToken = default)
        {
            _Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ExchangeName = exchangeName;
            _Connection = connectionFactory.CreateConnectionAsync().GetAwaiter().GetResult();
            _Channel = _Connection.CreateChannelAsync().GetAwaiter().GetResult();

            if (!ExchangeType.All().Contains(exchangeType))
            {
                throw new ArgumentException(
                    "the given exchange type is not valid, valid values are direct, fanout, headers and topic.",
                    nameof(exchangeType));
            }

            _Channel.ExchangeDeclareAsync(_ExchangeName, exchangeType, useDurableExchange, doAutoDeleteExchange)
                .GetAwaiter()
                .GetResult();

            _QueueName = _Channel.QueueDeclareAsync(queueName, useDurableQueue, useExclusiveQueue, autoDeleteQueue)
                .GetAwaiter()
                .GetResult()
                .QueueName;

            AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_Channel);
            consumer.ReceivedAsync += async (_, e) => await OnReceivedMessageAsync(e);

            consumer.RegisteredAsync += async (_, e) => _Logger.LogDebug(
                "Registered consumer {ConsumerId}",
                ToLogString(e.ConsumerTags));

            consumer.UnregisteredAsync += async (_, e) => _Logger.LogDebug(
                "Unregistered consumer {ConsumerId}",
                ToLogString(e.ConsumerTags));

            consumer.ShutdownAsync += async (_, e) => _Logger.LogDebug(
                "Shutdown consumer: {Code} ({Reason})",
                e.ReplyCode,
                e.ReplyText);

            _Channel.BasicConsumeAsync(_QueueName, true, consumer);
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
        public async Task SubscribeAsync(
            string topic,
            Func<ReadOnlyMemory<byte>, Task> messageReceiveAsync,
            CancellationToken cancellationToken = default)
        {
            if (_SubscriptionCallbacks.ContainsKey(topic.ToLowerInvariant()) == false)
            {
                _SubscriptionCallbacks.TryAdd(topic.ToLowerInvariant(), messageReceiveAsync);
                await _Channel.QueueBindAsync(_QueueName, _ExchangeName, topic, cancellationToken: cancellationToken);
                _Logger.LogInformation("Subscribed to topic '{Topic}'", topic);
            }
        }

        /// <inheritdoc />
        public async Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default)
        {
            _SubscriptionCallbacks.TryRemove(topic.ToLowerInvariant(), out _);
            await _Channel.QueueUnbindAsync(_QueueName, _ExchangeName, topic, cancellationToken: cancellationToken);
            _Logger.LogInformation("Unsubscribed from topic '{Topic}'", topic);
        }

        /// <inheritdoc />
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
            _Channel.CloseAsync().GetAwaiter().GetResult();
            _Connection.CloseAsync().GetAwaiter().GetResult();
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
