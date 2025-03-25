using System;

namespace OpenvpnNetClient.Exceptions
{
    public class CredsUnspecifiedError : Exception
    {
        public CredsUnspecifiedError(string msg) : base(msg) { }
    }
}
