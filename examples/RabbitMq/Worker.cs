using App.Metrics;
using App.Metrics.Meter;
using App.Metrics.Timer;
using BusLane.Consuming;
using BusLane.Producing;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMq
{
    public class Worker : BackgroundService
    {
        private readonly IMessageConsumer _MessageConsumer;
        private readonly IMessageProducer _MessageProducer;
        private readonly IMetrics _Metrics;
        private readonly TimerOptions _PublishTimer;
        private readonly MeterOptions _PublishMeter;
        private readonly MeterOptions _ConsumeMeter;

        public Worker(
            IMessageConsumer messageConsumer,
            IMessageProducer messageProducer,
            IMetrics metrics)
        {
            _MessageConsumer = messageConsumer ?? throw new ArgumentNullException(nameof(messageConsumer));
            _MessageProducer = messageProducer ?? throw new ArgumentNullException(nameof(messageProducer));
            _Metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));

            _PublishMeter = new MeterOptions
            {
                Name = "Publish Message/s",
                MeasurementUnit = Unit.Calls,
                RateUnit = TimeUnit.Seconds
            };

            _PublishTimer = new TimerOptions
            {
                Name = "Publish Message latency/ms",
                MeasurementUnit = Unit.Requests,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };
            
            _ConsumeMeter = new MeterOptions
            {
                Name = "Consume Message/s",
                MeasurementUnit = Unit.Calls,
                RateUnit = TimeUnit.Seconds
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _MessageConsumer.SubscribeAsync<TestMessage>("test", HandleMessage, stoppingToken);

            for (int taskCount = Environment.ProcessorCount; taskCount > 0; taskCount--)
            {
                await Task.Run(
                    () =>
                    {
                        while (!stoppingToken.IsCancellationRequested)
                        {
                            _Metrics.Measure.Timer.Time(
                                _PublishTimer,
                                async () => await PublishMessageAsync(stoppingToken));
                        }
                    },
                    stoppingToken);
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(10, stoppingToken);
            }
        }

        private async Task PublishMessageAsync(CancellationToken token)
        {
            await _MessageProducer.PublishAsync(
                "test",
                new TestMessage()
                {
                    Test = "foo"
                },
                token);
            
            _Metrics.Measure.Meter.Mark(_PublishMeter);
        }

        private async Task HandleMessage(TestMessage message, CancellationToken token)
        {
            await Task.Run(
                () =>
                {
                    _Metrics.Measure.Meter.Mark(_ConsumeMeter);
                },
                token);
        }
    }
}