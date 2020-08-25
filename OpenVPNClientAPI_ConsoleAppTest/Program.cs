using OpenVpnClientApi_CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class Program
    {
        private static readonly string _vpnBookConfigFileLocation = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\vpnbook-us2-tcp80.ovpn";
        private static readonly string _vpnBookUsername = "vpnbook";
        private static readonly string _vpnBookPassword = "Y6WtuUG";

        static void Main(string[] args)
        {
            Console.WriteLine("**Starting**");

            try
            {
                Client testClient = new Client();

                testClient.SetConfigWithFile(_vpnBookConfigFileLocation);

                testClient.AddCredentials(true, _vpnBookUsername, _vpnBookPassword);

                testClient.Connect();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press anything to exit");
            Console.ReadLine();
        }
    }
}
