using System.Data;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using RansauSysteme.Database.Exceptions;

namespace RansauSysteme.Database.Connection
{
    public class MySqlConnection : IDatabaseConnection
    {
        public DatabaseConfiguration DatabaseConfiguration { get; set; }

        public bool CanConnect { get; set; }

        public string ConnectionString => BuildConnectionString();

        private readonly ILogger<MySqlConnection> _logger;

        public MySqlConnection(ILogger<MySqlConnection> logger, DatabaseConfiguration? databaseConfiguration = null)
        {
            _logger = logger;
            CanConnect = false;
            DatabaseConfiguration = databaseConfiguration ?? DatabaseConfiguration.Localhost;
        }

        public IDbConnection CreateConnection()
        {
            if (string.IsNullOrEmpty(ConnectionString))
                throw new DatabaseConfigurationException("Cannot create connection if there is no configuration loaded");

            try
            {
                MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection(ConnectionString);
                connection.Open();

                CanConnect = true;
                return connection;
            }
            catch (MySqlException ex)
            {
                _logger.LogError(ex, ex.Message);
                throw new DatabaseConnectionException($"Failed to create MySQL connection: {ex.Message}", ex);
            }
        }

        public bool TestConnection()
        {
            try
            {
                using var connection = CreateConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                command.ExecuteScalar();

                // No error was thrown so the connection is valid
                CanConnect = true;
            }
            catch (Exception ex)
            {
                string logMessage = $"MySQL connection test failed for connection {ConnectionString}";
                _logger.LogWarning(ex, logMessage);
                CanConnect = false;
            }

            return CanConnect;
        }

        private string BuildConnectionString()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = DatabaseConfiguration.Server,
                Database = DatabaseConfiguration.Database,
                UserID = DatabaseConfiguration.Username,
                Password = DatabaseConfiguration.Password,
                Port = (uint)DatabaseConfiguration.Port,
                ConnectionTimeout = (uint)DatabaseConfiguration.ConnectionTimeout,
                SslMode = MySqlSslMode.Prefered
            };

            return builder.ConnectionString;
        }
    }
}