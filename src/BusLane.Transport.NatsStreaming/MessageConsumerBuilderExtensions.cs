using BusLane.Consuming;
using BusLane.Serializing;
using BusLane.Transport.NatsStreaming.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using STAN.Client;
using System.Threading;

namespace BusLane.Transport.NatsStreaming
{
    /// <summary>
    /// Contains extension methods for the <see cref="MessageConsumerBuilder"/> type.
    /// </summary>
    public static class MessageConsumerBuilderExtensions
    {
        /// <summary>
        /// Adds a RabbitMQ receiver to the Message consumer.
        /// </summary>
        /// <param name="builder">The <see cref="MessageConsumerBuilder"/>.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="queueGroup">NATS Queue group.</param>
        /// <param name="options">Connection options</param>
        /// <param name="cancellationToken">A token to cancel operations performed by the consumer with.</param>
        /// <returns>Configures the <see cref="MessageConsumerBuilder"/> with the NATS streaming server receivers.</returns>
        public static MessageConsumerBuilder AddNatsStreamingReceiver(
            this MessageConsumerBuilder builder,
            string clusterId,
            string clientId,
            string? queueGroup = null,
            StanOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            builder.UseMessageReceiver(
                new NatsStreamingMessageReceiver(
                    builder.LoggerFactory.CreateLogger<NatsStreamingMessageReceiver>(),
                    clusterId,
                    clientId,
                    queueGroup,
                    options,
                    cancellationToken));
            return builder;
        }

        /// <summary>
        /// Adds a message consumer with a RabbitMQ transport
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="queueGroup">NATS Queue group.</param>
        /// <param name="options">Connection options</param>
        /// <param name="messageDeserializer">A <see cref="IMessageDeserializer"/> to use.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A RabbitMQ message consumer</returns>
        public static IServiceCollection AddNatsStreamingMessageConsumer(
            this IServiceCollection services,
            string clusterId,
            string clientId,
            string? queueGroup = null,
            StanOptions? options = null,
            IMessageDeserializer? messageDeserializer = null,
            CancellationToken cancellationToken = default)
        {
            services.AddMessageConsumer(
                builder =>
                {
                    if (messageDeserializer != null)
                    {
                        builder.UseDeserializer(messageDeserializer);
                    }

                    builder.AddNatsStreamingReceiver(
                        clusterId,
                        clientId,
                        queueGroup,
                        options,
                        cancellationToken);
                });
            return services;
        }
    }
}
