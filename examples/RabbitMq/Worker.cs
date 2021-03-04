using System;
using System.Threading;
using System.Threading.Tasks;
using BusLane.Consuming;
using BusLane.Producing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RabbitMq
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _Logger;
        private readonly IMessageConsumer _MessageConsumer;
        private readonly IMessageProducer _MessageProducer;

        public Worker(ILogger<Worker> logger, IMessageConsumer messageConsumer, IMessageProducer messageProducer)
        {
            _Logger = logger;
            _MessageConsumer = messageConsumer;
            _MessageProducer = messageProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _MessageConsumer.SubscribeAsync<TestMessage>("test", HandleMessage, stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await _MessageProducer.PublishAsync(
                    "test",
                    new TestMessage()
                    {
                        Test = "foo"
                    },
                    stoppingToken);
                await Task.Delay(1, stoppingToken);
            }
        }

        private async Task HandleMessage(TestMessage message, CancellationToken token)
        {
            await Task.Run(
                () =>
                {
                    _Logger.LogInformation("Received message at {Timestamp:o}", DateTime.UtcNow);
                },
                token);
        }
    }
}