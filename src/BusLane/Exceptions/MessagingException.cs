using System;
using System.Runtime.Serialization;

namespace BusLane.Exceptions
{
    /// <summary>
    /// Indicates that an exception occurred while attempting to communicate with the message broker.
    /// </summary>
    public class MessagingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingException"/> class.
        /// </summary>
        public MessagingException()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public MessagingException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with a specified error message and a reference to
        /// the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public MessagingException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Initializes a new instance of the System.Exception class with serialized data.
        /// </summary>
        /// <param name="info">
        /// The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="StreamingContext"/> that contains contextual information about the source or destination.
        /// </param>
        /// <exception cref="SerializationException">
        /// The class name is null or <see cref="Exception.HResult"/>  is zero (0).
        /// </exception>
        protected MessagingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
