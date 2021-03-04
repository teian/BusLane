using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BusLane.Consuming
{
    public static class MessageConsumerBuilderExtensions
    {
        /// <summary>
        /// Adds a Message consumer to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add to.</param>
        /// <param name="consumerBuilder">The <see cref="MessageConsumerBuilder"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddMessageConsumer(
            this IServiceCollection services,
            Action<MessageConsumerBuilder> consumerBuilder)
        {
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            MessageConsumerBuilder builder =
                new MessageConsumerBuilder(serviceProvider.GetRequiredService<ILoggerFactory>());
            consumerBuilder(builder);
            services.AddSingleton(builder.Build());
            return services;
        }
    }
}