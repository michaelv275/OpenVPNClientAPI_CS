using System;

namespace OpenVpnClientApi_CS.Exceptions
{
    public class CredsUnspecifiedError : Exception
    {
        public CredsUnspecifiedError(String msg) : base(msg) { }
    }
}
