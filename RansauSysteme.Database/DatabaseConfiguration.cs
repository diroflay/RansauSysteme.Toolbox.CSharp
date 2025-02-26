using MySql.Data.MySqlClient;
using RansauSysteme.Database.Exceptions;

namespace RansauSysteme.Database
{
    /// <summary>
    /// Represents the configuration settings for a database connection.
    /// </summary>
    public class DatabaseConfiguration
    {
        /// <summary>
        /// Gets or sets the server hostname or IP address.
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the username for authenticating with the database server.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password for authenticating with the database server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the port number for connecting to the database server.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the connection timeout in seconds.
        /// </summary>
        public int ConnectionTimeout { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConfiguration"/> class
        /// with default empty values.
        /// </summary>
        public DatabaseConfiguration()
        {
            Server = string.Empty;
            Database = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            Port = -1;
            ConnectionTimeout = -1;
        }

        /// <summary>
        /// Gets a default configuration for connecting to a localhost MySQL database.
        /// </summary>
        /// <remarks>
        /// This is intended for development and testing purposes only.
        /// For production
        public static DatabaseConfiguration Localhost
            => new DatabaseConfiguration
            {
                Server = "localhost",
                Database = "local_database",
                Username = "root",
                Password = string.Empty,
                Port = 3306,
                ConnectionTimeout = 120
            };
    }
}