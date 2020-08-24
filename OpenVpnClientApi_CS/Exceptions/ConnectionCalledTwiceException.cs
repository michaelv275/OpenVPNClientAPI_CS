using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVpnClientApi_CS.Exceptions
{
    public class ConnectionCalledTwiceException : Exception
    {
        public ConnectionCalledTwiceException()
        {

        }

        public ConnectionCalledTwiceException(string message) : base(message) { }
    }
}
