using BusLane.Producing;
using BusLane.Serializing;
using BusLane.Transport.NatsStreaming.Publishing;
using Microsoft.Extensions.DependencyInjection;
using STAN.Client;

namespace BusLane.Transport.NatsStreaming
{
    /// <summary>
    /// Contains extension methods for the <see cref="MessageProducerBuilder"/> type.
    /// </summary>
    public static class MessageProducerBuilderExtensions
    {
        /// <summary>
        /// Adds a RabbitMQ message publisher to the publishers used when building a producer.
        /// </summary>
        /// <param name="builder">The builder to add to.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="options">Connection options</param>
        /// <returns>The builder.</returns>
        public static MessageProducerBuilder AddNatsStreamingPublisher(
            this MessageProducerBuilder builder,
            string clusterId,
            string clientId,
            StanOptions? options = null)
        {
            return builder.UsePublisher(
                new NatsStreamingMessagePublisher(
                    builder.CreateLogger<NatsStreamingMessagePublisher>(),
                    clusterId,
                    clientId,
                    options));
        }
        
        /// <summary>
        /// Adds a message consumer with a RabbitMQ transport
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="clusterId">The cluster ID of streaming server.</param>
        /// <param name="clientId">A unique ID for this connection.</param>
        /// <param name="options">Connection options</param>
        /// <param name="messageSerializer">A <see cref="IMessageSerializer"/> to use.</param>
        /// <returns>A RabbitMQ message consumer</returns>
        public static IServiceCollection AddNatsStreamingMessageProducer(
            this IServiceCollection services,
            string clusterId,
            string clientId,
            StanOptions? options = null,
            IMessageSerializer? messageSerializer = null)
        {
            services.AddMessageProducer(
                builder =>
                {
                    if (messageSerializer != null)
                    {
                        builder.UseSerializer(messageSerializer);
                    }

                    builder.AddNatsStreamingPublisher(
                        clusterId,
                        clientId,
                        options);
                });
            return services;
        }
    }
}