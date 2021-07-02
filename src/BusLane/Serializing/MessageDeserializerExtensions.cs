using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing
{
    /// <summary>
    /// Extensions for the <see cref="IMessageDeserializer"/>
    /// </summary>
    public static class MessageDeserializerExtensions
    {
        /// <summary>
        /// Deserializes an item from an array of bytes.
        /// </summary>
        /// <param name="deserializer">The <see cref="IMessageDeserializer"/>.</param>
        /// <param name="item">The item to deserialize.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The deserialized item.</returns>
        public static async Task<TMessage?> DeserializeAsync<TMessage>(
            this IMessageDeserializer deserializer,
            ReadOnlyMemory<byte> item,
            CancellationToken token = default)
        {
            using MemoryStream stream = new MemoryStream(item.ToArray(), false);
            return await deserializer.DeserializeAsync<TMessage>(stream, token);
        }
    }
}