using BusLane.Consuming;

namespace BusLane.Serializing.Json
{
    /// <summary>
    /// JSON deserializer extensions to use the default <see cref="System.Text.Json"/> implementation
    /// </summary>
    public static class JsonMessageDeserializerExtensions
    {
        /// <summary>
        /// Registers the default JSON deserializer for the builder
        /// </summary>
        /// <param name="builder">The <see cref="MessageConsumer"/> builder</param>
        /// <returns>the builder instance</returns>
        public static MessageConsumerBuilder UseJsonMessageDeserializer(this MessageConsumerBuilder builder)
        {
            builder.UseDeserializer(new JsonMessageDeserializer());
            return builder;
        }
    }
}