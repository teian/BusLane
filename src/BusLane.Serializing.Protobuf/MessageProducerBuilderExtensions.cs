using BusLane.Producing;

namespace BusLane.Serializing.MessagePack
{
    /// <summary>
    /// Contains extension methods for the <see cref="MessageProducerBuilder"/> type.
    /// </summary>
    public static class MessageProducerBuilderExtensions
    {
        /// <summary>
        /// Sets a serializer to use when publishing messages of a specific type.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns>This builder.</returns>
        public static MessageProducerBuilder UseProtobufSerializer(this MessageProducerBuilder builder)
        {
            return builder.UseSerializer(new ProtobufMessageSerializer());
        }
    }
}
