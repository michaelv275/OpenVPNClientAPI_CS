using OpenVpnClientApi_CS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class Program
    {
        static readonly string _enclaveConfigLocation = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\enclave.ovpn";
        static readonly string _vpnBookConfigFileLocation = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\vpnbook-us1-tcp80.ovpn";
        private static readonly string _vpnBookUsername = "vpnbook";
        private static readonly string _vpnBookPassword = "Y6WtuUG";

        public static List<Thread> VPNThreads = new List<Thread>();
        public static Client VPNManager = new Client();

        static void Main(string[] args)
        {
            Console.WriteLine("**Starting**");

            try
            {
                VPNManager.ConnectionAttemptCompleted += VPNManager_ConnectionAttemptCompleted;
                VPNManager.ConnectionClosed += VPNManager_ConnectionClosed;

                RunNewConnection(_vpnBookConfigFileLocation);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Press anything to exit");
            Console.ReadLine();
        }

        private static void VPNManager_ConnectionClosed(object sender, EventArgs e)
        {
            Console.WriteLine("Connection closed event was fired");
        }

        private static void VPNManager_ConnectionAttemptCompleted(object sender, EventArgs e)
        {
            VPNManager.ConnectionAttemptCompleted -= VPNManager_ConnectionAttemptCompleted;
            VPNManager.ConnectionAttemptCompleted += VPNManager_ConnectionAttemptCompleted2;
            Console.WriteLine("The connection event has fired.");

            Task.Run(() => { Task.Delay(5000).Wait(); }).Wait();

            RunNewConnection(_enclaveConfigLocation);
        }

        private static void VPNManager_ConnectionAttemptCompleted2(object sender, EventArgs e)
        {
            VPNManager.ConnectionAttemptCompleted += VPNManager_ConnectionAttemptCompleted2;
            Console.WriteLine("The second connection event has fired.");
        }

        private static void RunNewConnection(object configFile)
        {
            string fileLocation = configFile.ToString();


            VPNManager.SetConfigWithFile(fileLocation);

            if (fileLocation == _enclaveConfigLocation)
            {
                VPNManager.AddCredentials(false);
            }
            else
            {
                VPNManager.AddCredentials(true, _vpnBookUsername, _vpnBookPassword);
            }

            VPNManager.Connect();
        }
    }
}
