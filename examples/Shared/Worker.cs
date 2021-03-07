using BusLane.Consuming;
using BusLane.Producing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Shared
{
    public class Worker : BackgroundService
    {
        private readonly WorkerOptions _Options;
        private readonly IMessageConsumer _MessageConsumer;
        private readonly IMessageProducer _MessageProducer;

        public Worker(
            IOptions<WorkerOptions> options,
            IMessageConsumer messageConsumer,
            IMessageProducer messageProducer)
        {
            _Options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _MessageConsumer = messageConsumer ?? throw new ArgumentNullException(nameof(messageConsumer));
            _MessageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _MessageConsumer.SubscribeAsync<TestMessage>("test", HandleMessage, stoppingToken);

            List<Task> messageProducerWorker = new List<Task>();
            for (int taskCount = _Options.NumberOfTasks; taskCount > 0; taskCount--)
            {
                messageProducerWorker.Add(Task.Run(() => PublishMessageAsync(stoppingToken), stoppingToken));
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10, stoppingToken);
            }
        }

        private async Task PublishMessageAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await _MessageProducer.PublishAsync(
                    "test",
                    new TestMessage()
                    {
                        Test = "foo"
                    },
                    token);
            }
        }

        private Task HandleMessage(TestMessage message, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}