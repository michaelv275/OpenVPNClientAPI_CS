using System;
namespace OpenVPNClientAPI_ConsoleAppTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the test/example project for the C# OpenVPN client API wrapper.");
            Console.WriteLine("Feel free to take a look at the example classes by Changing the Startup Object for the OpenVPNClientAPI_ConsoleAppTest");

            Console.WriteLine("The SingleConnection_Example will create a single VPN connection, then print the stats, then close after a set amount of time.");
            Console.WriteLine();

            Console.Write("The SwitchConnecion_Example will create one VPN connection, print the stats, close, then switch to a second VPN connection; ");
            Console.Write("printing the stats, then closing the second connection as well");

            Console.WriteLine();
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }
    }
}
