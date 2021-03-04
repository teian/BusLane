using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Serializing
{
    /// <summary>
    /// Serializes objects into bytes.
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Serializes an item and writes it into a specified stream.
        /// </summary>
        /// <param name="item">The item to serialize</param>
        /// <param name="target">The stream to write to.</param>
        /// <param name="token">A token to cancel the operation with.</param>
        Task SerializeAsync<TMessage>(TMessage item, Stream target, CancellationToken token = default);
    }
}