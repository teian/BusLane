using BusLane.Producing;

namespace BusLane.Serializing.MessagePack
{
    public static class MessagePackMessageSerializerExtensions
    {
        public static MessageProducerBuilder UseMessagePackMessageDeserializer(this MessageProducerBuilder builder)
        {
            builder.UseSerializer(new MessagePackMessageSerializer());
            return builder;
        }
    }
}