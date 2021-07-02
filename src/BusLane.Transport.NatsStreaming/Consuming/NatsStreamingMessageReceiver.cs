using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BusLane.Exceptions;
using Microsoft.Extensions.Logging;
using STAN.Client;

namespace BusLane.Transport.NatsStreaming.Consuming
{
    /// <summary>
    /// A <see cref="IMessageReceiver"/> that reads transient messages from the nats streaming server message broker.
    /// If an error occurs during message handling, the message will not be re-consumed.
    /// </summary>
    internal sealed class NatsStreamingMessageReceiver : IMessageReceiver
    {
        /// <summary>
        /// A cancellation token source used to abort operation.
        /// </summary>
        private readonly CancellationTokenSource _Cancellation;

        private readonly ConcurrentDictionary<string, Func<ReadOnlyMemory<byte>, Task>> _SubscriptionCallbacks =
            new ConcurrentDictionary<string, Func<ReadOnlyMemory<byte>, Task>>();

        private readonly ConcurrentDictionary<string, IStanSubscription> _Subscriptions =
            new ConcurrentDictionary<string, IStanSubscription>();

        /// <summary>
        /// Gets the logger to write to.
        /// </summary>
        private readonly ILogger<NatsStreamingMessageReceiver> _Logger;

        /// <summary>
        /// Gets the connection to the message broker.
        /// </summary>
        private readonly IStanConnection _Connection;

        /// <summary>
        /// The cluster ID of streaming server
        /// </summary>
        private readonly string _ClusterId;

        /// <summary>
        /// A unique ID for this connection
        /// </summary>
        private readonly string _CLientId;

        /// <summary>
        /// The queue group load balances messages between all consumers belonging to the same group.
        /// </summary>
        private readonly string? _QueueGroup;

        /// <summary>
        /// Marker if disposal of the receiver was triggered
        /// </summary>
        private bool _IsDisposing;

        /// <summary>
        /// Initializes a new <see cref="NatsStreamingMessageReceiver"/>
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="queueGroup">NATS Queue group.</param>
        /// <param name="options">Connection options</param>
        /// <param name="cancellationToken">A token to cancel operations performed by the consumer with.</param>
        public NatsStreamingMessageReceiver(
            ILogger<NatsStreamingMessageReceiver> logger,
            string clusterId,
            string clientId,
            string? queueGroup = null,
            StanOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _Cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _Logger = logger;
            _Connection = new StanConnectionFactory().CreateConnection(
                clusterId,
                clientId,
                options ?? StanOptions.GetDefaultOptions());
            _ClusterId = clusterId;
            _CLientId = clientId;
            _QueueGroup = queueGroup;
        }

        private async Task HandleMessage(string topic, ReadOnlyMemory<byte> rawMessage)
        {
            if (_SubscriptionCallbacks.ContainsKey(topic))
            {
                await _SubscriptionCallbacks[topic].Invoke(rawMessage);
            }
        }

        private async Task OnReceivedMessageAsync(StanMsgHandlerArgs eventArguments)
        {
            try
            {
                _Logger.LogDebug(
                    "Received message on '{Subject}' with sequence {Sequence}",
                    eventArguments.Message.Subject,
                    eventArguments.Message.Sequence);

                _Logger.LogTrace(
                    "Received message: body:=byte[{BodyLength}], Subject:={Subject}, "
                    + "Sequence:={Sequence}, Redelivered:={Redelivered}, "
                    + "TimeStamp:={TimeStamp}",
                    eventArguments.Message.Data.Length,
                    eventArguments.Message.Subject,
                    eventArguments.Message.Sequence,
                    eventArguments.Message.Redelivered,
                    eventArguments.Message.TimeStamp);

                await HandleMessage(eventArguments.Message.Subject, eventArguments.Message.Data);
            }
            catch (OperationCanceledException)
            {
                _Logger.LogWarning("Cancelled message consumption");
            }
            catch (Exception e)
            {
                _Logger.LogError(
                    e,
                    "Error while receiving message on '{Subject}' with sequence '{Sequence}'",
                    eventArguments.Message.Subject,
                    eventArguments.Message.Sequence);
            }
        }

        /// <inheritdoc />
        public Task SubscribeAsync(
            string topic,
            Func<ReadOnlyMemory<byte>, Task> messageReceiveAsync,
            CancellationToken cancellationToken = default)
        {
            if (_SubscriptionCallbacks.ContainsKey(topic) == false)
            {
                IStanSubscription subscription;
                if (string.IsNullOrWhiteSpace(_QueueGroup) == false)
                {
                    subscription = _Connection.Subscribe(
                        topic,
                        _QueueGroup,
                        async (_, args) => await OnReceivedMessageAsync(args));
                }
                else
                {
                    subscription = _Connection.Subscribe(
                        topic,
                        async (_, args) => await OnReceivedMessageAsync(args));
                }

                _Subscriptions.TryAdd(topic, subscription);
                _SubscriptionCallbacks.TryAdd(topic, messageReceiveAsync);
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
            _Subscriptions[topic].Close();
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
                    "Discarded dispose of the {InstanceName} instance with "
                    + "Client Id '{ClientId}' and Cluster Id '{ClusterId}' (already triggered)",
                    GetType().FullName,
                    _CLientId,
                    _ClusterId);
                return;
            }

            _Logger.LogTrace(
                "Dispose of the {InstanceName} instance with Client Id '{ClientId}' and Cluster Id '{ClusterId}' started",
                GetType().FullName,
                _CLientId,
                _ClusterId);

            _IsDisposing = true;
            _Cancellation.Cancel();
            _Connection.Close();
            _Connection.Dispose();
            _Cancellation.Dispose();

            _Logger.LogTrace(
                "Dispose of the {InstanceName} instance with Client Id '{ClientId}' and Cluster Id '{ClusterId}' completed",
                GetType().FullName,
                _CLientId,
                _ClusterId);
        }
    }
}