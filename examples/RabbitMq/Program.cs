using BusLane.Transport.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

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
                        services.AddOptions<WorkerOptions>().Bind(hostContext.Configuration.GetSection("Worker"));
                        
                        string rmqHost = hostContext.Configuration.GetValue("Rabbitmq:HostName", "127.0.0.1");
                        int rmqPort = hostContext.Configuration.GetValue("Rabbitmq:Port", 5672);
                        string rmqUser = hostContext.Configuration.GetValue("Rabbitmq:Username", "guest");
                        string rmqPassword = hostContext.Configuration.GetValue("Rabbitmq:Password", "guest");
                        
                        services.AddRabbitMqMessageProducer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = rmqHost;
                                connectionConfig.Port = rmqPort;
                                connectionConfig.UserName = rmqUser;
                                connectionConfig.Password = rmqPassword;
                            });

                        services.AddRabbitMqMessageConsumer(
                            connectionConfig =>
                            {
                                connectionConfig.HostName = rmqHost;
                                connectionConfig.Port = rmqPort;
                                connectionConfig.UserName = rmqUser;
                                connectionConfig.Password = rmqPassword;
                            });

                        services.AddHostedService<Worker>();
                    });
    }
}