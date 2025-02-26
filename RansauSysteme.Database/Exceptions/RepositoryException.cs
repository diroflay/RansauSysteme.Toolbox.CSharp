﻿using System;

namespace RansauSysteme.Database.Exceptions
{
    public class RepositoryException : Exception
    {
        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException(string message, Exception innerException)
            : base(message, innerException) { }

        public RepositoryException() : base()
        {
        }
    }
}