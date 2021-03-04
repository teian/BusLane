using MessagePack;

namespace RabbitMq
{
    [MessagePackObject]
    public class TestMessage
    {
        [Key(0)]
        public string Test { get; set; }
    }
}