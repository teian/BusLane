using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BusLane.Producing
{
    /// <summary>
    /// <see cref="MessageProducerBuilder"/> extensions
    /// </summary>
    public static class MessageProducerBuilderExtensions
    {
        /// <summary>
        /// Adds a message producer to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="producerBuilder">The <see cref="MessageProducerBuilder"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddMessageProducer(
            this IServiceCollection services,
            Action<MessageProducerBuilder> producerBuilder)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            MessageProducerBuilder builder =
                new MessageProducerBuilder(serviceProvider.GetRequiredService<ILoggerFactory>());
            producerBuilder(builder);
            services.AddSingleton(builder.Build());
            return services;
        }
    }
}