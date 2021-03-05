using BusLane.Consuming;

namespace BusLane.Serializing.MessagePack
{
    /// <summary>
    /// Contains extension methods for the <see cref="MessageConsumerBuilder"/> type.
    /// </summary>
    public static class MessageConsumerBuilderExtensions
    {
        /// <summary>
        /// Uses the stated deserializer for messages when building consumers.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>This builder.</returns>
        public static MessageConsumerBuilder UseProtobufDeserializer(this MessageConsumerBuilder builder)
        {
            return builder.UseDeserializer(new ProtobufMessageDeserializer());
        }
    }
}
