using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing.Json
{
    /// <summary>
    /// A <see cref="IMessageDeserializer"/> that serializes items into a binary format.
    /// </summary>
    public sealed class JsonMessageDeserializer : IMessageDeserializer
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
            return await JsonSerializer.DeserializeAsync<TMessage>(source, cancellationToken: token);
        }
    }
}