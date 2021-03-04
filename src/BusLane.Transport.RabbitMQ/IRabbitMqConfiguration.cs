using RabbitMQ.Client;

namespace BusLane.Transport.RabbitMQ
{
    /// <summary>
    /// Represents a configuration for setting up a <see cref="IConnectionFactory"/>.
    /// </summary>
    public interface IRabbitMqConfiguration
    {
        /// <summary>
        /// Gets the host to connect to.
        /// </summary>
        public string? Host { get; }

        /// <summary>
        /// Gets the port to connect to.
        /// </summary>
        public int? Port { get; }

        /// <summary>
        /// Gets the username to use when connecting.
        /// </summary>
        public string? Username { get; }

        /// <summary>
        /// Gets the password to use when connecting.
        /// </summary>
        public string? Password { get; }
    }
}
