using BusLane.Consuming;

namespace BusLane.Serializing.MessagePack
{
    /// <summary>
    /// Extension to register the MessagePack deserializer in the <see cref="MessageConsumerBuilder"/>
    /// </summary>
    public static class MessagePackMessageDeserializerExtensions
    {
        /// <summary>
        /// Registers the MessagePack deserializer for the builder
        /// </summary>
        /// <param name="builder">The <see cref="MessageConsumerBuilder"/></param>
        /// <returns>the builder instance</returns>
        public static MessageConsumerBuilder UseMessagePackMessageDeserializer(this MessageConsumerBuilder builder)
        {
            builder.UseDeserializer(new MessagePackMessageDeserializer());
            return builder;
        }
    }
}