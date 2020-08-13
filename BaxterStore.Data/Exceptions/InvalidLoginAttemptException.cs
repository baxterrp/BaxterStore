using System;

namespace BaxterStore.Data.Exceptions
{
    public class InvalidLoginAttemptException : Exception
    {
        public InvalidLoginAttemptException(string message) : base(message) { }
    }
}
