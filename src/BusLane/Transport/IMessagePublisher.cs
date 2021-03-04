using BusLane.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Transport
{
    /// <summary>
    /// Publishes serialized messages to a message broker.
    /// </summary>
    public interface IMessagePublisher : IDisposable
    {
        /// <summary>
        /// Publishes a new message to the broker, using the stated topic.
        /// </summary>
        /// <param name="payload">The payload to publish.</param>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        Task PublishAsync(
            ReadOnlyMemory<byte> payload,
            string topic,
            CancellationToken cancellationToken = default);
    }
}
