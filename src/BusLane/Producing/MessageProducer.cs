using BusLane.Exceptions;
using BusLane.Serializing;
using BusLane.Transport;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Producing
{
    /// <summary>
    /// The default implementation of an <see cref="IMessageProducer"/> that is built by an
    /// <see cref="MessageProducerBuilder"/>.
    /// </summary>
    public sealed class MessageProducer : IMessageProducer
    {
        /// <summary>
        /// Gets the logger to write to.
        /// </summary>
        private readonly ILogger<MessageProducer> _Logger;

        /// <summary>
        /// The serializer to serialize the messages.
        /// </summary>
        private readonly IMessageSerializer _Serializer;

        /// <summary>
        /// The publisher to publish messages to.
        /// </summary>
        private readonly IMessagePublisher _Publisher;

        /// <summary>
        /// Initializes a new <see cref="MessageProducer"/>.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="publisher">The publishers to use for publishing messages.</param>
        /// <param name="serializer">The serializer to serialize the messages.</param>
        public MessageProducer(
            ILogger<MessageProducer> logger,
            IMessagePublisher publisher,
            IMessageSerializer serializer)
        {
            _Logger = logger;
            _Publisher = publisher;
            _Serializer = serializer;
        }

        /// <summary>
        /// Publishes a new message to the broker, using the stated topics.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="message">The message to publish.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        public async Task PublishAsync<TMessage>(
            string topic,
            TMessage message,
            CancellationToken cancellationToken = default)
        {
            _Logger.LogTrace("Publishing message of type '{MessageType}'", typeof(TMessage).FullName);

            ReadOnlyMemory<byte> serializedMessage = await _Serializer.SerializeAsync(message, cancellationToken);

            await PublishMessageAsync(serializedMessage, topic, cancellationToken);
            _Logger.LogTrace(
                "Published message of type '{MessageType}' to topic '{Topic}'",
                typeof(TMessage).FullName,
                topic);
        }

        /// <summary>
        /// Publishes a serialized message to the broker, using the stated topics.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        private async Task PublishMessageAsync(
            ReadOnlyMemory<byte> message,
            string topic,
            CancellationToken cancellationToken)
        {
            try
            {
                await _Publisher.PublishAsync(message, topic, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception publishException)
            {
                throw new MessagingException("Failed to publish message.", publishException);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public void Dispose()
        {
            _Publisher.Dispose();
        }
    }
}