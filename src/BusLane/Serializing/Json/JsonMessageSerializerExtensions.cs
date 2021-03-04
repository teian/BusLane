using BusLane.Producing;

namespace BusLane.Serializing.Json
{
    public static class JsonMessageSerializerExtensions
    {
        public static MessageProducerBuilder UseJsonMessageDeserializer(this MessageProducerBuilder builder)
        {
            builder.UseSerializer(new JsonMessageSerializer());
            return builder;
        }
    }
}