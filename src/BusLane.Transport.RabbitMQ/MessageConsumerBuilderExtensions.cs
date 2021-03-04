using BusLane.Consuming;
using BusLane.Serializing;
using BusLane.Transport.RabbitMQ.Consuming;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;

namespace BusLane.Transport.RabbitMQ
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
        /// <param name="configureConnection">Configures the RabbitMQ connection factory</param>
        /// <param name="exchangeName">The exchange to use</param>
        /// <param name="useDurableExchange">Whether the exchange is durable or not</param>
        /// <param name="doAutoDeleteExchange">Automatically delete the exchange if no queues are bound.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>The RabbitMQ configures <see cref="MessageConsumerBuilder"/>.</returns>
        public static MessageConsumerBuilder AddRabbitMqReceiver(
            this MessageConsumerBuilder builder,
            Action<ConnectionFactory> configureConnection,
            string exchangeName = Constants.DefaultExchange,
            bool useDurableExchange = true,
            bool doAutoDeleteExchange = false,
            CancellationToken cancellationToken = default)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            configureConnection(connectionFactory);
            
            builder.UseMessageReceiver(
                new RabbitMqMessageReceiver(
                    builder.LoggerFactory.CreateLogger<RabbitMqMessageReceiver>(),
                    connectionFactory,
                    exchangeName,
                    useDurableExchange,
                    doAutoDeleteExchange,
                    cancellationToken));
            return builder;
        }

        /// <summary>
        /// Adds a message consumer with a RabbitMQ transport
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="configureConnection">Configures the RabbitMQ connection factory</param>
        /// <param name="exchangeName">The exchange to use</param>
        /// <param name="useDurableExchange">Whether the exchange is durable or not</param>
        /// <param name="doAutoDeleteExchange">Automatically delete the exchange if no queues are bound.</param>
        /// <param name="messageDeserializer">A <see cref="IMessageDeserializer"/> to use.</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A RabbitMQ message consumer</returns>
        public static IServiceCollection AddRabbitMqMessageConsumer(
            this IServiceCollection services,
            Action<ConnectionFactory> configureConnection,
            string exchangeName = Constants.DefaultExchange,
            bool useDurableExchange = true,
            bool doAutoDeleteExchange = false,
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

                    builder.AddRabbitMqReceiver(
                        configureConnection,
                        exchangeName,
                        useDurableExchange,
                        doAutoDeleteExchange,
                        cancellationToken);
                });
            return services;
        }
    }
}
