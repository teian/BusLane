using System;
using BusLane.Serializing;
using BusLane.Serializing.Json;
using BusLane.Transport;
using Microsoft.Extensions.Logging;

namespace BusLane.Consuming
{
    /// <summary>
    /// A builder for instances of <see cref="IMessageConsumer"/>.
    /// </summary>
    public sealed class MessageConsumerBuilder
    {
        private IMessageDeserializer _Deserializer;
        private IMessageReceiver? _MessageReceiver;
        public ILoggerFactory LoggerFactory { get; }

        /// <summary>
        /// Initializes a new <see cref="MessageConsumerBuilder"/>.
        /// </summary>
        /// <param name="loggerFactory">The factory to create logger from.</param>
        public MessageConsumerBuilder(ILoggerFactory loggerFactory)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _Deserializer = new JsonMessageDeserializer();
        }

        /// <summary>
        /// Uses the stated deserializer to decode messaged from the message receiver.
        /// </summary>
        /// <returns>This builder.</returns>
        public MessageConsumerBuilder UseDeserializer(IMessageDeserializer deserializer)
        {
            _Deserializer = deserializer;
            return this;
        }
        
        /// <summary>
        /// Uses the stated handler for exceptions thrown while parsing a raw message.
        /// </summary>
        /// <returns>This builder.</returns>
        public MessageConsumerBuilder UseMessageReceiver(IMessageReceiver messageReceiver)
        {
            _MessageReceiver = messageReceiver;
            return this;
        }

        /// <summary>
        /// Builds a new <see cref="IMessageConsumer"/> with a specified message handler.
        /// </summary>
        /// <returns>A new message consumer that uses the stated message handler.</returns>
        public IMessageConsumer Build()
        {
            if (_MessageReceiver is null)
            {
                throw new ArgumentNullException(
                    nameof(_MessageReceiver),
                    "No message receiver set, cannot build a new message consumer.");
            }

            MessageConsumer messageConsumer = new MessageConsumer(
                LoggerFactory.CreateLogger<MessageConsumer>(),
                _MessageReceiver,
                _Deserializer);

            return messageConsumer;
        }
    }
}