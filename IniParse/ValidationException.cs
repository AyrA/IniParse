using System;
using System.Runtime.Serialization;

namespace IniParse
{
    /// <summary>
    /// Provides an exception for validation errors
    /// </summary>
    [Serializable]
    public class ValidationException : Exception
    {
        public ValidationException() : this("Unspecified validation error")
        {
        }

        public ValidationException(string message) : base(message)
        {
        }

        public ValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}