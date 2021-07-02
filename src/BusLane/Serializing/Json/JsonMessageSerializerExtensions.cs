using BusLane.Producing;

namespace BusLane.Serializing.Json
{
    /// <summary>
    /// JSON serializer extensions to use the default <see cref="System.Text.Json"/> implementation
    /// </summary>
    public static class JsonMessageSerializerExtensions
    {
        /// <summary>
        /// Registers the default JSON serializer for the builder
        /// </summary>
        /// <param name="builder">The <see cref="MessageProducer"/> builder</param>
        /// <returns>the builder instance</returns>
        public static MessageProducerBuilder UseJsonMessageDeserializer(this MessageProducerBuilder builder)
        {
            builder.UseSerializer(new JsonMessageSerializer());
            return builder;
        }
    }
}