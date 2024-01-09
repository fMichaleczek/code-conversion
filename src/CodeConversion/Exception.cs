using System;

namespace CodeConversion
{
    public class IncompleteCodeBlockException : Exception
    {
        public IncompleteCodeBlockException() : base()
        {
        }

        public IncompleteCodeBlockException(string message) : base(message)
        {
        }

        public IncompleteCodeBlockException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
