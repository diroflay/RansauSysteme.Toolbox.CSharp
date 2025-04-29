﻿namespace RansauSysteme.Database.Exceptions
{
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message) : base(message)
        {
        }

        public DatabaseConnectionException(string message, Exception innerException)
            : base(message, innerException) { }

        public DatabaseConnectionException() : base()
        {
        }
    }
}