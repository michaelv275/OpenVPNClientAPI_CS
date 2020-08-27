using OpenVpnClientApi_CS;
using OpenVpnClientApi_CS.Exceptions;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class SwitchConnection_Example
    {
        static readonly string _config1 = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\vpnbook-us1-tcp80.ovpn";
        static readonly string _config2 = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\enclave.ovpn";
        private static readonly string _vpnCredUsername = "vpnbook";
        private static readonly string _vpnCredPassword = "Y6WtuUG";

        //Be sure to set this if your VPN server requires authentication
        private static bool _vpnUsesCredentialAuth = true;

        //This test will allow the connection to be alive 60 seconds before stopping. 
        private static readonly int _vpnConnectionDurationSeconds = 150;

        //Thi will hold the new config location after the first one is stopped
        private static string _switchConnectionConfigLocation;

        public static Client VPNManager = new Client();

        /// <summary>
        /// A simple example that will start a connection using the provided config string or file in config1, then switch to config 2.
        /// The connection (if successful) will be auto terminated after _vpnConnectionDurationSeconds seconds, thanks to
        /// the ConnectionEstablished event that is subscribed to.
        /// 
        /// Normally, this would not all be handled at once with event handlers. That is just to allow for ease of execution. The normal flow should be:
        /// 1.) start VPN connection 1 with VPNManager.Connect(config1)
        /// 2.) Close the connection when you want to switch with VPNManager.Stop()
        /// 3.) start the new Connection with VPNManager.Connect(config2)
        /// 
        /// If the connection is not stopped deliberately, it will stay active until the program ends, or connection is lost somehow (server down, tunnel destroyed, etc...)
        /// If a second Connect(someConfig) call is started before ending the first, a OpenVPNClientApi_CS.Exceptions.ConnectionCalledTwiceException will be thrown.
        /// 
        /// It is recommended that all events are subscribed to and handled, otherwise their output will be written to the Console.
        /// </summary>
        static void Main(string[] args)
        {
            Console.WriteLine("**Starting**");

            try
            {
                VPNManager.ConnectionEstablished += VPNManager_ConnectionEstablished;
                VPNManager.ConnectionClosed += VPNManager_ConnectionClosed;
                //VPNManager.CoreEventReceived += Custom Core Event Handler
                //VPNManager.LogReceived += Custom Logging Event Handler

                RunNewConnection(_config1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void VPNManager_ConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Connection closed event was fired. The VPN is disconnected");
        }

        private static void VPNManager_SecondConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("The second connection closed event was fired. The VPN is disconnected");

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void VPNManager_SwitchConnections(object sender, EventArgs e)
        {
            Console.WriteLine("Switch connection event was fired");
            VPNManager.ConnectionClosed -= VPNManager_SwitchConnections;

            if (!String.IsNullOrEmpty(_switchConnectionConfigLocation))
            {
                RunNewConnection(_switchConnectionConfigLocation);
            }
        }

        //This will wait _vpnConnectionDurationSeconds seconds after establishing the first connection to start the second.
        private static void VPNManager_ConnectionEstablished(object sender, EventArgs e)
        {
            //it is important to unsubscribe events if you do not want them to fire every time
            VPNManager.ConnectionEstablished -= VPNManager_ConnectionEstablished;
            VPNManager.ConnectionEstablished += VPNManager_SecondConnectionEstablished;
            Console.WriteLine("The first connection completed event has fired. The VPN is connected");
            Console.WriteLine("The connection will be alive for {0} seconds", _vpnConnectionDurationSeconds);

            Console.WriteLine();
            VPNManager.Show_stats();

            Task.Delay(_vpnConnectionDurationSeconds * 1000).Wait(); 

            RunNewConnection(_config2);
        }

        //This will fire once the second connection is established, wait the same amount of time, then stop the VPN.
        private static void VPNManager_SecondConnectionEstablished(object sender, EventArgs e)
        {
            //Under normal circumstances, the events would not be used like this. This is just for execution simplicity
            VPNManager.ConnectionEstablished -= VPNManager_SecondConnectionEstablished;
            VPNManager.ConnectionClosed -= VPNManager_ConnectionClosed;
            VPNManager.ConnectionClosed += VPNManager_SecondConnectionClosed;
            Console.WriteLine("The second VPN connection has been established.");
            Console.WriteLine("The connection will be alive for {0} seconds", _vpnConnectionDurationSeconds);

            Console.WriteLine();
            VPNManager.Show_stats();

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

            //The username and password parameters are optional, and only used if the first parameter is true
            VPNManager.AddCredentials(_vpnUsesCredentialAuth, _vpnCredUsername, _vpnCredPassword);

            try
            {
                VPNManager.Connect();
            }
            catch (ConnectionCalledTwiceException tooManyConnections)
            {
                Console.WriteLine(tooManyConnections);

                VPNManager.ConnectionClosed += VPNManager_SwitchConnections;
                _switchConnectionConfigLocation = configData;

                VPNManager.Stop();
            }
            catch (Exception otherEx)
            {
                Console.WriteLine(otherEx);
            }
        }
    }
}
