using OpenVpnClientApi_CS;
using OpenVpnClientApi_CS.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class SingleConnection_Example
    {
        static readonly string _vpnConfig = @"File Location or config string";
        private static readonly string _vpnCredUsername = "username";
        private static readonly string _vpnCredPassword = "password";

        //Be sure to set this if your VPN server requires authentication
        private static bool _vpnUsesCredentialAuth = false;

        //This test will allow the connection to be alive 60 seconds before stopping. 
        private static readonly int _vpnConnectionDurationSeconds = 20;

        public static Client VPNManager = new Client();

        /// <summary>
        /// A simple example that will start a connection using the provided config string or file.
        /// The connection (if successful) will be auto terminated after _vpnConnectionDurationSeconds seconds, thanks to
        /// the ConnectionEstablished event that is subscribed to.
        /// 
        /// If the connection is not stopped deliberately, it will stay active until the program ends, or connection is lost somehow (server down, tunnel destroyed, etc...)
        /// 
        /// It is recommended that all events are subscribed to and handled, otherwise their output will be written to the Console.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("**Starting**");

            try
            {
                VPNManager.ConnectionEstablished += VPNManager_ConnectionEstablished;
                VPNManager.ConnectionClosed += VPNManager_ConnectionClosed; ;
                //VPNManager.CoreEventReceived += Custom Core Event Handler
                //VPNManager.LogReceived += Custom Logging Event Handler

                RunNewConnection(_vpnConfig);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void VPNManager_ConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine();
            Console.WriteLine("The VPN connection has been closed.");

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void VPNManager_ConnectionEstablished(object sender, EventArgs e)
        {
            Console.WriteLine("The connection established event has fired. and the VPN is connected.");
            Console.WriteLine();

            VPNManager.Show_stats();
            Console.WriteLine();

            Task.Delay(_vpnConnectionDurationSeconds * 1000).Wait();

            VPNManager.Stop();
        }

        private static void RunNewConnection(string configData)
        {
            if (File.Exists(configData))
            {
                VPNManager.SetConfigWithFile(configData);
            }
            else
            {
                VPNManager.SetConfigWithMultiLineString(configData);
            }

            if (!_vpnUsesCredentialAuth)
            {
                var x = VPNManager.AddCredentials(false);
            }
            else
            {
                VPNManager.AddCredentials(true, _vpnCredUsername, _vpnCredPassword);
            }

            try
            {
                VPNManager.Connect();
            }
            catch (ConnectionCalledTwiceException tooManyConnections)
            {
                Console.WriteLine(tooManyConnections);
            }
            catch (Exception otherEx)
            {
                Console.WriteLine(otherEx);
            }
        }
    }
}
