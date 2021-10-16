using System;

namespace TaskManager.Exceptions
{
    class CustomException : Exception
    {
        public CustomException(string message) : base(message)
        {

        }
    }
}
