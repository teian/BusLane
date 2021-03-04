using BusLane.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BusLane.Producing
{
    /// <summary>
    /// A producer that publishes messages to a message broker.
    /// </summary>
    public interface IMessageProducer : IDisposable
    {
        /// <summary>
        /// Publishes a new message to the broker, using the stated topics.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message being published.</typeparam>
        /// <param name="topic">The topic to publish the message to.</param>
        /// <param name="message">The message to publish.</param> 
        /// <param name="cancellationToken">The token to cancel the operation with.</param>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled.</exception>
        /// <exception cref="MessagingException">Thrown if communication with the message broker failed.</exception>
        Task PublishAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default);
    }
}