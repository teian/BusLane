using MessagePack;

namespace Shared
{
    [MessagePackObject]
    public class TestMessage
    {
        [Key(0)]
        public string Test { get; set; }
    }
}