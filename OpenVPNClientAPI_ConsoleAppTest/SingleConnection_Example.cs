using System;
using System.IO;
using System.Threading.Tasks;
using OpenVpnClientApi_CS;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class SingleConnection_Example
    {
        static readonly string _vpnConfig = "file location or config string";
        private static readonly string _vpnCredUsername = "username";
        private static readonly string _vpnCredPassword = "password";

        //Be sure to set this if your VPN server requires authentication
        private static bool _vpnUsesCredentialAuth = false;

        public static Client VPNManager = new Client();

        /// <summary>
        /// A simple example that will start a connection using the provided config string or file.
        /// The connection (if successful) will be terminated after sending the "stop" command in the terminal
        /// 
        /// It is also important to know that an internet connection will not be established. I don't know why. But it only seems to work once the library is made
        /// into a nuget package and then used. Running thi example wil allow you to see that the connection is made and data can be sent, but trying to access the internet 
        /// does not work.
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
                VPNManager.ConnectionClosed += VPNManager_ConnectionClosed;
                //VPNManager.ConnectionTimedOut += Custom Connection Timeout handler
                //VPNManager.CoreEventReceived += Custom Core Event Handler
                VPNManager.LogReceived += (s, e) => Console.WriteLine(String.Format("Log received event : {0}", e));

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

            CheckForStop();
        }

        private async static void CheckForStop()
        {
            await Task.Run(() => 
            { 
                bool isExiting = false;

                while (!isExiting)
                {
                    Console.WriteLine("Waiting for \"stop\" command");
                    string command = Console.ReadLine().ToLower();

                    switch (command)
                    {
                        case "stop":
                            VPNManager.Stop();
                            isExiting = true;
                            break;
                        default:
                            break;
                    }
                }
            });
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
                VPNManager.AddCredentials(false);
            }
            else
            {
                VPNManager.AddCredentials(true, _vpnCredUsername, _vpnCredPassword);
            }

            try
            {
                VPNManager.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
