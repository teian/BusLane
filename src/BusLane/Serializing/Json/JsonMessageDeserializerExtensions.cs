using BusLane.Consuming;

namespace BusLane.Serializing.Json
{
    public static class JsonMessageDeserializerExtensions
    {
        public static MessageConsumerBuilder UseJsonMessageDeserializer(this MessageConsumerBuilder builder)
        {
            builder.UseDeserializer(new JsonMessageDeserializer());
            return builder;
        }
    }
}