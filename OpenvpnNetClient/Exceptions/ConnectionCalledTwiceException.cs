using System;

namespace OpenvpnNetClient.Exceptions
{
    public class ConnectionCalledTwiceException : Exception
    {
        public ConnectionCalledTwiceException()
        {

        }

        public ConnectionCalledTwiceException(string message) : base(message) { }
    }
}
