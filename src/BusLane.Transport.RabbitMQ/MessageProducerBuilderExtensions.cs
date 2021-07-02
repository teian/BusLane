using System;
using BusLane.Producing;
using BusLane.Serializing;
using BusLane.Transport.RabbitMQ.Publishing;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace BusLane.Transport.RabbitMQ
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
        /// <param name="connectionConfiguration">Configures the RabbitMQ connection factory.</param>
        /// <param name="exchangeName">The name of the exchange to use.</param>
        /// <param name="exchangeType">RabbitMQ specific exchange types (direct|fanout|headers|topic)</param>
        /// <param name="useDurableExchange">Indicates whether the exchange for durable messages should be used.</param>
        /// <param name="doAutoDeleteExchange">Deletes the exchange when the last channel leaves.</param>
        /// <returns>The builder.</returns>
        public static MessageProducerBuilder AddRabbitMqPublisher(
            this MessageProducerBuilder builder,
            Action<ConnectionFactory> connectionConfiguration,
            string exchangeName = Constants.DefaultExchange,
            string exchangeType = ExchangeType.Topic,
            bool useDurableExchange = true,
            bool doAutoDeleteExchange = false)
        {
            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionConfiguration(connectionFactory);

            return builder.UsePublisher(
                new RabbitMqMessagePublisher(
                    builder.CreateLogger<RabbitMqMessagePublisher>(),
                    connectionFactory,
                    exchangeName,
                    exchangeType,
                    useDurableExchange,
                    doAutoDeleteExchange));
        }
        
        /// <summary>
        /// Adds a message consumer with a RabbitMQ transport
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="configureConnection">Configures the RabbitMQ connection factory</param>
        /// <param name="exchangeName">The exchange to use</param>
        /// <param name="exchangeType">RabbitMQ specific exchange types (direct|fanout|headers|topic)</param>
        /// <param name="useDurableExchange">Whether the exchange is durable or not</param>
        /// <param name="doAutoDeleteExchange">Automatically delete the exchange if no queues are bound.</param>
        /// <param name="messageSerializer">A <see cref="IMessageSerializer"/> to use.</param>
        /// <returns>A RabbitMQ message consumer</returns>
        public static IServiceCollection AddRabbitMqMessageProducer(
            this IServiceCollection services,
            Action<ConnectionFactory> configureConnection,
            string exchangeName = Constants.DefaultExchange,
            string exchangeType = ExchangeType.Topic,
            bool useDurableExchange = true,
            bool doAutoDeleteExchange = false,
            IMessageSerializer? messageSerializer = null)
        {
            services.AddMessageProducer(
                builder =>
                {
                    if (messageSerializer != null)
                    {
                        builder.UseSerializer(messageSerializer);
                    }

                    builder.AddRabbitMqPublisher(
                        configureConnection,
                        exchangeName,
                        exchangeType,
                        useDurableExchange,
                        doAutoDeleteExchange);
                });
            return services;
        }
    }
}