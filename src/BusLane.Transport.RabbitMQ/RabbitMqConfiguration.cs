namespace BusLane.Transport.RabbitMQ
{
    /// <summary>
    /// Default implementation of an <see cref="IRabbitMqConfiguration"/>.
    /// </summary>
    public sealed class RabbitMqConfiguration : IRabbitMqConfiguration
    {
        /// <summary>
        /// Gets or sets the host to connect to.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Gets or sets the port to connect to.
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// Gets or sets the username to use when connecting.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets the password to use when connecting.
        /// </summary>
        public string? Password { get; set; }
    }
}
