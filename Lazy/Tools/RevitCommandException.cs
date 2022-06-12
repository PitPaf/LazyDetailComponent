using System;

namespace pza.Tools
{
    internal class RevitCommandException : Exception
    {
        public RevitCommandException() : base() { }
        public RevitCommandException(string message) : base(message) { }
        public RevitCommandException(string message, Exception inner) : base(message, inner) { }
       

    }

}
