using BusLane.Consuming;

namespace BusLane.Serializing.MessagePack
{
    public static class MessagePackMessageDeserializerExtensions
    {
        public static MessageConsumerBuilder UseMessagePackMessageDeserializer(this MessageConsumerBuilder builder)
        {
            builder.UseDeserializer(new MessagePackMessageDeserializer());
            return builder;
        }
    }
}