using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing.Json
{
    /// <summary>
    /// A <see cref="IMessageSerializer"/> that serializes items into a binary format.
    /// </summary>
    public sealed class JsonMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// Serializes the stated item and writes it to a target stream.
        /// </summary>
        /// <param name="item">The item to serialize.</param>
        /// <param name="target">The stream to write to.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        public async Task SerializeAsync<TMessage>(
            TMessage item,
            Stream target,
            CancellationToken token = default)
        {
            await JsonSerializer.SerializeAsync(target, item, cancellationToken: token).ConfigureAwait(false);
        }
    }
}