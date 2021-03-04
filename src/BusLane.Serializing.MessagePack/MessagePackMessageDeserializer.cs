using MessagePack;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing.MessagePack
{
    /// <summary>
    /// A <see cref="IMessageDeserializer"/> that serializes items into a binary format.
    /// </summary>
    public sealed class MessagePackMessageDeserializer : IMessageDeserializer
    {
        /// <summary>
        /// Reads and deserializes an item from a stream and returns it.
        /// </summary>
        /// <param name="source">The stream to read from.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        public async Task<TMessage?> DeserializeAsync<TMessage>(
            Stream source,
            CancellationToken token = default)
        {
            return await MessagePackSerializer.DeserializeAsync<TMessage>(source, cancellationToken: token);
        }
    }
}