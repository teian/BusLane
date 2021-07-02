using BusLane.Producing;

namespace BusLane.Serializing.MessagePack
{
    /// <summary>
    /// MessagePack serializer extensions for the <see cref="MessageProducerBuilder"/>
    /// </summary>
    public static class MessagePackMessageSerializerExtensions
    {
        /// <summary>
        /// Registers the MessagePack serializer for the builder
        /// </summary>
        /// <param name="builder">The <see cref="MessageProducer"/> builder</param>
        /// <returns>the builder instance</returns>
        public static MessageProducerBuilder UseMessagePackMessageSerializer(this MessageProducerBuilder builder)
        {
            builder.UseSerializer(new MessagePackMessageSerializer());
            return builder;
        }
    }
}