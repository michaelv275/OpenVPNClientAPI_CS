using System;

namespace OpenvpnNetClient.Exceptions
{
    public class ConfigError : Exception
    {
        public ConfigError(string msg) : base(msg) { }
    }
}
