using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Transport
{
    /// <summary>
    /// A generic message receiver
    /// </summary>
    public interface IMessageReceiver : IDisposable
    {
        /// <summary>
        /// Subscribes to a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="messageReceiveAsync"></param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        Task SubscribeAsync(
            string topic,
            Func<ReadOnlyMemory<byte>, Task> messageReceiveAsync,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribes from a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        Task UnsubscribeAsync(string topic, CancellationToken cancellationToken = default);
    }
}