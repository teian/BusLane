using App.Metrics;
using App.Metrics.AspNetCore;
using BusLane.Serializing.Json;
using BusLane.Serializing.MessagePack;
using BusLane.Transport.RabbitMQ;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
                .ConfigureMetrics(
                    builder =>
                    {
                        builder.Report.ToConsole(
                            options => { options.FlushInterval = TimeSpan.FromSeconds(5); });
                    })
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