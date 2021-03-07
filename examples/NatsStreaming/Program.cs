using BusLane.Transport.NatsStreaming;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;
using STAN.Client;

namespace NatsStreaming
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
                        
                        string host = hostContext.Configuration.GetValue("Nats:Host", "127.0.0.1");
                        int port = hostContext.Configuration.GetValue("Nats:Port", 4222);

                        StanOptions optins = StanOptions.GetDefaultOptions();
                        optins.NatsURL = $"nats://{host}:{port}";
                        
                        services.AddNatsStreamingMessageProducer("test-cluster", "test-producer", optins);
                        services.AddNatsStreamingMessageConsumer("test-cluster", "test-consumner", "test-queue", optins);

                        services.AddHostedService<Worker>();
                    });
    }
}