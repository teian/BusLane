using BusLane.Serializing.MessagePack;
using BusLane.Transport.RabbitMQ;
using Microsoft.Extensions.Configuration;
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
                        string rmqHost = hostContext.Configuration.GetValue("Rabbitmq:HostName", "127.0.0.1");
                        int rmqPort = hostContext.Configuration.GetValue("Rabbitmq:Port", 5672);
                        
                        services.AddRabbitMqMessageProducer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = rmqHost;
                                connectionConfig.Port = rmqPort;
                            },
                            messageSerializer: new MessagePackMessageSerializer());

                        services.AddRabbitMqMessageConsumer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = rmqHost;
                                connectionConfig.Port = rmqPort;
                            },
                            messageDeserializer: new MessagePackMessageDeserializer());

                        services.AddHostedService<Worker>();
                    });
    }
}