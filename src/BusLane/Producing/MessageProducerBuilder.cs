using System;
using BusLane.Serializing;
using BusLane.Serializing.Json;
using BusLane.Transport;
using Microsoft.Extensions.Logging;

namespace BusLane.Producing
{
    /// <summary>
    /// A builder for <see cref="IMessageProducer"/>.
    /// </summary>
    public sealed class MessageProducerBuilder
    {
        private IMessageSerializer _Serializer;

        private IMessagePublisher? _Publisher;
        private readonly ILoggerFactory _LoggerFactory;

        /// <summary>
        /// Initializes a new <see cref="MessageProducerBuilder"/>.
        /// </summary>
        /// <param name="loggerFactory">The factory to create logger from.</param>
        public MessageProducerBuilder(ILoggerFactory loggerFactory)
        {
            _LoggerFactory = loggerFactory;
            _Serializer = new JsonMessageSerializer();
        }
        
        /// <summary>
        /// Creates a logger for a given type
        /// </summary>
        /// <typeparam name="TLogger">The type of the logger should be created for</typeparam>
        /// <returns>A logger for the given type</returns>
        public ILogger<TLogger> CreateLogger<TLogger>()
        {
            return _LoggerFactory.CreateLogger<TLogger>();
        }

        /// <summary>
        /// Uses the publisher in the producer.
        /// </summary>
        /// <param name="publisher">The publisher to use.</param>
        /// <returns>This builder.</returns>
        public MessageProducerBuilder UsePublisher(IMessagePublisher publisher)
        {
            _Publisher = publisher;
            return this;
        }

        /// <summary>
        /// Uses the stated serializer for messages in the producer.
        /// </summary>
        /// <param name="serializer">The serializer to use.</param>
        /// <returns>This builder.</returns>
        public MessageProducerBuilder UseSerializer(IMessageSerializer serializer)
        {
            _Serializer = serializer;
            return this;
        }


        /// <summary>
        /// Builds an <see cref="IMessageProducer"/>, based on the current state of the builder.
        /// </summary>
        /// <returns></returns>
        public IMessageProducer Build()
        {
            if (_Publisher is null)
            {
                throw new ArgumentNullException(nameof(_Publisher), "No publisher was set, cannot build the producer.");
            }

            return new MessageProducer(
                _LoggerFactory.CreateLogger<MessageProducer>(),
                _Publisher,
                _Serializer);
        }
    }
}