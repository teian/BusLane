using BusLane.Serializing.Json;
using BusLane.Serializing.MessagePack;
using BusLane.Transport.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RabbitMq
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(
                    (hostContext, services) =>
                    {
                        services.AddRabbitMqMessageProducer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = "127.0.0.1";
                                connectionConfig.Port = 5672;
                            }, 
                            messageSerializer: new MessagePackMessageSerializer());
                        
                        services.AddRabbitMqMessageConsumer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = "127.0.0.1";
                                connectionConfig.Port = 5672;
                            },
                            messageDeserializer: new MessagePackMessageDeserializer());
                        
                        services.AddHostedService<Worker>();
                    });
    }
}