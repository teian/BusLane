using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing
{
    /// <summary>
    /// Deserializes message objects from bytes.
    /// </summary>
    public interface IMessageDeserializer
    {
        /// <summary>
        /// Reads an item from a specified stream and deserializes it.
        /// </summary>
        /// <param name="source">A stream to read from.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        /// <returns>The deserialized item.</returns>
        Task<TMessage?> DeserializeAsync<TMessage>(Stream source, CancellationToken token = default);
    }
}