using System.Data;

namespace RansauSysteme.Database.Connection
{
    /// <summary>
    /// Defines the contract for database connection providers.
    /// </summary>
    public interface IDatabaseConnection
    {
        /// <summary>
        /// Creates a new database connection.
        /// </summary>
        /// <returns>An open database connection ready for use.</returns>
        public IDbConnection CreateConnection();

        /// <summary>
        /// Tests if the connection can be established with the current configuration.
        /// </summary>
        /// <returns>True if the connection can be established; otherwise, false.</returns>
        public bool TestConnection();

        /// <summary>
        /// Gets a value indicating whether the connection can be established.
        /// </summary>
        public bool CanConnect { get; }

        /// <summary>
        /// Gets the connection string used to connect to the database.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the database configuration used to build the connection string.
        /// </summary>
        public DatabaseConfiguration DatabaseConfiguration { get; set; }
    }
}