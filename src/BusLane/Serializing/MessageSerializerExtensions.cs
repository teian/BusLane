using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing
{
    /// <summary>
    /// Extensions for the <see cref="IMessageSerializer"/>
    /// </summary>
    public static class MessageSerializerExtensions
    {
        /// <summary>
        /// Serializes an item into an array of bytes.
        /// </summary>
        /// <param name="serializer">The <see cref="IMessageSerializer"/>.</param>
        /// <param name="item">The item to serialize.</param>
        /// <param name="cancellationToken">A token to cancel the operation with.</param>
        /// <returns>The serialized item.</returns>
        public static async Task<ReadOnlyMemory<byte>> SerializeAsync<TMessage>(
            this IMessageSerializer serializer,
            TMessage item,
            CancellationToken cancellationToken = default)
        {
            using MemoryStream stream = new MemoryStream();
            await serializer.SerializeAsync(item, stream, cancellationToken);
            return stream.ToArray();
        }
    }
}