using BusLane.Exceptions;
using Microsoft.Extensions.Logging;
using STAN.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Transport.NatsStreaming.Publishing
{
    /// <summary>
    /// An <see cref="IMessagePublisher"/> that publishes messages to a RabbitMQ broker.
    /// </summary>
    internal sealed class NatsStreamingMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<NatsStreamingMessagePublisher> _Logger;

        /// <summary>
        /// Gets the connection to the message broker.
        /// </summary>
        private readonly IStanConnection _Connection;

        /// <summary>
        /// Initializes a new <see cref="NatsStreamingMessagePublisher"/>.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="options">Connection options</param>
        public NatsStreamingMessagePublisher(
            ILogger<NatsStreamingMessagePublisher> logger,
            string clusterId,
            string clientId,
            StanOptions? options = null)
        {
            _Logger = logger;
            _Connection = new StanConnectionFactory().CreateConnection(
                clusterId,
                clientId,
                options ?? StanOptions.GetDefaultOptions());
        }

        /// <summary>
        /// Publishes a new message to the broker, using the stated categories.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="T:System.OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="T:Mes.Core.Libraries.Messaging.Exceptions.CommunicationException">
        /// Thrown if communication with the message broker failed.
        /// </exception>
        public async Task PublishAsync(
            ReadOnlyMemory<byte> message,
            string topic,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _Logger.LogTrace("Publishing message to topic '{Topic}'", topic);

            try
            {
                await _Connection.PublishAsync(topic, message.ToArray());
                _Logger.LogDebug("Published to topic '{Topic}'", topic);
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
            _Connection.Close();
            _Connection.Dispose();
        }
    }
}