namespace RansauSysteme.Database.Exceptions
{
    public class DatabaseConfigurationException : Exception
    {
        public DatabaseConfigurationException(string message) : base(message)
        {
        }

        public DatabaseConfigurationException(string message, Exception innerException)
            : base(message, innerException) { }

        public DatabaseConfigurationException() : base()
        {
        }
    }
}