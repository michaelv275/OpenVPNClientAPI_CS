using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using OpenVpnClientApi_CS;
using OpenVpnClientApi_CS.Exceptions;

namespace OpenVPNClientAPI_UnitTest
{
    [TestClass]
    public class ClientTests
    {
        /// The config is coming from https://www.vpnbook.com/freevpn. They change their passwords often, so you may have to redownload
        /// a new config bundle and change the username/password
        private static readonly string _vpnBookConfig = "";
        private static readonly string _vpnBookConfigFileLocation = "";
        private static readonly string _vpnBookUsername = "";
        private static readonly string _vpnBookPassword = "";

        [TestMethod]
        public void InitializeCoreLibrary_Test()
        {
            Client testClient = null;

            try
            {
                testClient = new Client();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not initialize Client object.");
                Debug.WriteLine(ex);
            }

            Assert.IsNotNull(testClient.ConfigData);
        }

        /// <summary>
        /// Tests setting a config with a VPN config in a string.
        /// </summary>
        [TestMethod]
        public void SetConfigWithString_Test()
        {
            Client testClient = null;

            try
            {
                testClient = new Client();
                testClient.SetConfigWithMultiLineString(_vpnBookConfig);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Config could not be set.");
                Debug.WriteLine(ex);
            }

            Assert.IsTrue(!String.IsNullOrEmpty(testClient.ConfigData.content));
        }

        [TestMethod]
        public void SetConfigWithFile_Test()
        {
            Client testClient = null;

            try
            {
                testClient = new Client();
                testClient.SetConfigWithFile(_vpnBookConfigFileLocation);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not initialize Client object.");
                Debug.WriteLine(ex);
            }

            Assert.IsTrue(!String.IsNullOrEmpty(testClient.ConfigData.content));
        }

        [TestMethod]
        public void AddCredentials_Test()
        {
            Client testClient = null;
            ClientAPI_Status returnStatus = null;

            try
            {
                testClient = new Client();
                testClient.SetConfigWithFile(_vpnBookConfigFileLocation);

                returnStatus = testClient.AddCredentials(true, _vpnBookUsername, _vpnBookPassword);
            }
            catch (CredsUnspecifiedError ex)
            {
                Debug.WriteLine("Could not Set VPN credentials");
                Debug.WriteLine(ex);
            }

            Assert.IsTrue(returnStatus != null && !returnStatus.error);
        }

        [TestMethod]
        public void Connect_Test()
        {
            Client testClient = null;

            try
            {
                testClient = new Client();
                testClient.SetConfigWithFile(_vpnBookConfigFileLocation);

                ClientAPI_Status credStatus = testClient.AddCredentials(true, _vpnBookUsername, _vpnBookPassword);

                if (!credStatus.error)
                {
                    testClient.Connect();
                }
            }
            catch (CredsUnspecifiedError ex)
            {
                Debug.WriteLine("Could not Set VPN credentials");
                Debug.WriteLine(ex);
            }
        }
    }
}
