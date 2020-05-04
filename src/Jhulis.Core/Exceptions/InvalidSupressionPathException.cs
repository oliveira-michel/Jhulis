using System;

namespace Jhulis.Core.Exceptions
{
    public class InvalidSupressionPathException : Exception
    {
        public InvalidSupressionPathException(string message) : base(message) { }
    }
}
