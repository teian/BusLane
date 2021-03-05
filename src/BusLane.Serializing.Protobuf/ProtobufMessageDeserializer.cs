using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace BusLane.Serializing.Protobuf
{
    /// <summary>
    /// An <see cref="IMessageDeserializer"/> for Protobuf messages.
    /// </summary>
    public sealed class ProtobufMessageDeserializer : IMessageDeserializer
    {
        /// <summary>
        /// Deserializes a message and returns it.
        /// </summary>
        /// <param name="source">A stream to read from.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The deserialized message object.</returns>
        public Task<TMessage> DeserializeAsync<TMessage>(Stream source, CancellationToken token = default)
        {
            return Task.FromResult(Serializer.Deserialize<TMessage>(source));
        }
        
    }
}
