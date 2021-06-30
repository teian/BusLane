using BusLane.Exceptions;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Transport.RabbitMQ.Publishing
{
    /// <summary>
    /// An <see cref="IMessagePublisher"/> that publishes messages to a RabbitMQ broker.
    /// </summary>
    internal sealed class RabbitMqMessagePublisher : IMessagePublisher
    {
        private readonly ILogger<RabbitMqMessagePublisher> _Logger;
        private readonly bool _UseDurableExchange;
        private readonly string _Exchange;
        private readonly IModel _Channel;
        private readonly IConnection _Connection;

        /// <summary>
        /// Initializes a new <see cref="RabbitMqMessagePublisher"/>.
        /// </summary>
        /// <param name="logger">The logger to write to.</param>
        /// <param name="connectionFactory">The factory to create connections to the broker from.</param>
        /// <param name="exchange">The name of the exchange to use</param>
        /// <param name="exchangeType">RabbitMQ specific exchange types (direct|fanout|headers|topic)</param>
        /// <param name="useDurableExchange">Indicates whether the exchange for durable messages should be used.</param>
        /// <param name="doAutoDeleteExchange">Deletes the exchange when the last channel leaves.</param>
        public RabbitMqMessagePublisher(
            ILogger<RabbitMqMessagePublisher> logger,
            IConnectionFactory connectionFactory,
            string exchange = Constants.DefaultExchange,
            string exchangeType = ExchangeType.Topic,
            bool useDurableExchange = true,
            bool doAutoDeleteExchange = false)
        {
            _Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Exchange = exchange;
            _UseDurableExchange = useDurableExchange;
            _Connection = connectionFactory.CreateConnection();
            _Channel = _Connection.CreateModel();

            if (!ExchangeType.All().Contains(exchangeType))
            {
                throw new ArgumentException(
                    "the given exchange type is not valid, valid values are direct, fanout, headers and topic.",
                    nameof(exchangeType));
            }
            
            _Channel.ExchangeDeclare(_Exchange, exchangeType, _UseDurableExchange, doAutoDeleteExchange);
        }

        /// <summary>
        /// Publishes a new message to the broker, using the stated categories.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="T:System.OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="T:Mes.Core.Libraries.Messaging.Exceptions.CommunicationException">
        /// Thrown if communication with the message broker failed.
        /// </exception>
        public async Task PublishAsync(
            ReadOnlyMemory<byte> message,
            string topic,
            CancellationToken cancellationToken = new CancellationToken())
        {
            _Logger.LogTrace("Publishing message to topic '{Topic}'", topic);

            try
            {
                await Task.Run(() =>
                    {
                        IBasicProperties properties = _Channel.CreateBasicProperties();
                        properties.Persistent = _UseDurableExchange;
                        properties.DeliveryMode = _UseDurableExchange ? (byte) 2 : (byte) 1;
                        _Channel.BasicPublish(_Exchange, topic, false, properties, message);
                    },
                    cancellationToken);

                _Logger.LogDebug("Published to topic '{Topic}'", topic);
            }
            catch (Exception publishException)
            {
                throw new MessagingException("Failed to publish message.", publishException);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources
        /// asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous dispose operation.</returns>
        public void Dispose()
        {
            _Channel.Close();
            _Channel.Dispose();
            _Connection.Close();
            _Connection.Dispose();
        }
    }
}