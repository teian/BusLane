using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProtoBuf;

namespace BusLane.Serializing.Protobuf
{
    /// <summary>
    /// An <see cref="IMessageSerializer"/> for Protobuf messages.
    /// </summary>
    public sealed class ProtobufMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Serializes a message into bytes.
        /// </summary>
        /// <param name="item">The message to serialize.</param>
        /// <param name="target">The stream to write to.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        public async Task SerializeAsync<TMessage>(
            TMessage item,
            Stream target,
            CancellationToken token = default)
        {
            await Task.Run(() => Serializer.Serialize(target, item), token).ConfigureAwait(false);
        }
    }
}
