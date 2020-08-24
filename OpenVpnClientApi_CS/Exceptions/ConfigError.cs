using System;

namespace OpenVpnClientApi_CS.Exceptions
{
    public class ConfigError : Exception
    {
        public ConfigError(string msg) : base(msg) { }
    }
}
