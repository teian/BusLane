using BusLane.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Consuming
{
    public interface IMessageConsumer
    {
        /// <summary>
        /// Subscribes to a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to subscribe to.</param>
        /// <param name="messageReceiveAsync">Callback function for received messages</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        Task SubscribeAsync<TMessage>(
            string topic,
            Func<TMessage?, CancellationToken, Task> messageReceiveAsync,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Unsubscribes from a topic on the broker.
        /// </summary>
        /// <param name="topic">The topic to unsubscribe from.</param>
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        Task UnsubscribeAsync<TMessage>(string topic, CancellationToken cancellationToken = default);
    }
}