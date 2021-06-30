using BusLane.Exceptions;
using BusLane.Serializing;
using BusLane.Transport;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Consuming
{
    /// <summary>
    /// An
    /// </summary>
    internal class MessageConsumer : IMessageConsumer, IDisposable
    {
        private readonly ILogger _Logger;

        private readonly IMessageDeserializer _Deserializer;

        private readonly IMessageReceiver _MessageReceivers;

        private readonly ConcurrentDictionary<string, Type> _TopicSubscriptions;

        /// <summary>
        /// Initializes a new <see cref="MessageConsumer"/>.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="deserializer">The deserializer to use for messages.</param>
        /// <param name="messageReceiver">.</param>
        public MessageConsumer(
            ILogger<MessageConsumer> logger,
            IMessageReceiver messageReceiver,
            IMessageDeserializer deserializer)
        {
            _Logger = logger;
            _MessageReceivers = messageReceiver;
            _Deserializer = deserializer;
            _TopicSubscriptions = new ConcurrentDictionary<string, Type>();
        }

        /// <summary>
        /// Subscribes to a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="messageReceiveCallback"></param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        public async Task SubscribeAsync<TMessage>(
            string topic,
            Func<TMessage, CancellationToken, Task> messageReceiveCallback,
            CancellationToken cancellationToken = default)
        {
            if (_TopicSubscriptions.ContainsKey(topic.ToLowerInvariant()) == false)
            {
                if (_TopicSubscriptions.TryAdd(topic.ToLowerInvariant(), typeof(TMessage)))
                {
                    await _MessageReceivers.SubscribeAsync(
                        topic,
                        async (rawMessage) =>
                        {
                            try
                            {
                                TMessage? decodedMessage =
                                    await _Deserializer.DeserializeAsync<TMessage>(rawMessage, cancellationToken);
                                
                                if (decodedMessage != null)
                                {
                                    await messageReceiveCallback.Invoke(decodedMessage, cancellationToken);
                                }
                            }
                            catch(Exception ex)
                            {
                                _Logger.LogError(ex, "Failed to consume a message");
                            }                            
                        },
                        cancellationToken);
                }
            }
            else
            {
                _Logger.LogWarning("Subscription for {Topic} already registered", topic);
            }
        }

        /// <summary>
        /// Unsubscribes from a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        public async Task UnsubscribeAsync<TMessage>(string topic, CancellationToken cancellationToken = default)
        {
            if (_TopicSubscriptions.ContainsKey(topic)
                && _TopicSubscriptions.TryRemove(topic.ToLowerInvariant(), out _))
            {
                await _MessageReceivers.UnsubscribeAsync(topic, cancellationToken);
            }
        }


        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public void Dispose()
        {
            _MessageReceivers.Dispose();
        }
    }
}