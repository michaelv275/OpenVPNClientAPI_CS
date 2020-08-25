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
        private static readonly string _vpnBookConfig = "client\ndev tun3\nproto tcp\nremote 198.7.58.147 80\nremote us2.vpnbook.com 80\nresolv-retry infinite\nnobind\npersist-key\npersist-tun\nauth-user-pass\ncomp-lzo\nverb 3\ncipher AES-128-CBC\nfast-io\npull\nroute-delay 2\nredirect-gateway\n<ca>\n-----BEGIN CERTIFICATE-----\nMIIDyzCCAzSgAwIBAgIJAKRtpjsIvek1MA0GCSqGSIb3DQEBBQUAMIGgMQswCQYD\nVQQGEwJDSDEPMA0GA1UECBMGWnVyaWNoMQ8wDQYDVQQHEwZadXJpY2gxFDASBgNV\nBAoTC3ZwbmJvb2suY29tMQswCQYDVQQLEwJJVDEUMBIGA1UEAxMLdnBuYm9vay5j\nb20xFDASBgNVBCkTC3ZwbmJvb2suY29tMSAwHgYJKoZIhvcNAQkBFhFhZG1pbkB2\ncG5ib29rLmNvbTAeFw0xMzA0MjQwNDA3NDhaFw0yMzA0MjIwNDA3NDhaMIGgMQsw\nCQYDVQQGEwJDSDEPMA0GA1UECBMGWnVyaWNoMQ8wDQYDVQQHEwZadXJpY2gxFDAS\nBgNVBAoTC3ZwbmJvb2suY29tMQswCQYDVQQLEwJJVDEUMBIGA1UEAxMLdnBuYm9v\nay5jb20xFDASBgNVBCkTC3ZwbmJvb2suY29tMSAwHgYJKoZIhvcNAQkBFhFhZG1p\nbkB2cG5ib29rLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEAyNwZEYs6\nWN+j1zXYLEwiQMShc1mHmY9f9cx18hF/rENG+TBgaS5RVx9zU+7a9X1P3r2OyLXi\nWzqvEMmZIEhij8MtCxbZGEEUHktkbZqLAryIo8ubUigqke25+QyVLDIBuqIXjpw3\nhJQMXIgMic1u7TGsvgEUahU/5qbLIGPNDlUCAwEAAaOCAQkwggEFMB0GA1UdDgQW\nBBRZ4KGhnll1W+K/KJVFl/C2+KM+JjCB1QYDVR0jBIHNMIHKgBRZ4KGhnll1W+K/\nKJVFl/C2+KM+JqGBpqSBozCBoDELMAkGA1UEBhMCQ0gxDzANBgNVBAgTBlp1cmlj\naDEPMA0GA1UEBxMGWnVyaWNoMRQwEgYDVQQKEwt2cG5ib29rLmNvbTELMAkGA1UE\nCxMCSVQxFDASBgNVBAMTC3ZwbmJvb2suY29tMRQwEgYDVQQpEwt2cG5ib29rLmNv\nbTEgMB4GCSqGSIb3DQEJARYRYWRtaW5AdnBuYm9vay5jb22CCQCkbaY7CL3pNTAM\nBgNVHRMEBTADAQH/MA0GCSqGSIb3DQEBBQUAA4GBAKaoCEWk2pitKjbhChjl1rLj\n6FwAZ74bcX/YwXM4X4st6k2+Fgve3xzwUWTXinBIyz/WDapQmX8DHk1N3Y5FuRkv\nwOgathAN44PrxLAI8kkxkngxby1xrG7LtMmpATxY7fYLOQ9yHge7RRZKDieJcX3j\n+ogTneOl2w6P0xP6lyI6\n-----END CERTIFICATE-----\n</ca>\n<cert>\n-----BEGIN CERTIFICATE-----\nMIID6DCCA1GgAwIBAgIBATANBgkqhkiG9w0BAQUFADCBoDELMAkGA1UEBhMCQ0gx\nDzANBgNVBAgTBlp1cmljaDEPMA0GA1UEBxMGWnVyaWNoMRQwEgYDVQQKEwt2cG5i\nb29rLmNvbTELMAkGA1UECxMCSVQxFDASBgNVBAMTC3ZwbmJvb2suY29tMRQwEgYD\nVQQpEwt2cG5ib29rLmNvbTEgMB4GCSqGSIb3DQEJARYRYWRtaW5AdnBuYm9vay5j\nb20wHhcNMTMwNTA2MDMyMTIxWhcNMjMwNTA0MDMyMTIxWjB4MQswCQYDVQQGEwJD\nSDEPMA0GA1UECBMGWnVyaWNoMQ8wDQYDVQQHEwZadXJpY2gxFDASBgNVBAoTC3Zw\nbmJvb2suY29tMQ8wDQYDVQQDEwZjbGllbnQxIDAeBgkqhkiG9w0BCQEWEWFkbWlu\nQHZwbmJvb2suY29tMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCkTM/8E+JH\nCjskqMIwgYDrNCBTWZLa+qKkJjZ/rliJomTfVYwKwv1AHYYU6RHpCxS1qFp3BEKL\nvQlASuzycSv1FGnNiLmg94fqzzWdmjs1XWosnLqbOwxx2Ye/1WoakSHia0pItoZk\nxK7/fllm42+Qujri/ERGga5Cb/TfiP6pUQIDAQABo4IBVzCCAVMwCQYDVR0TBAIw\nADAtBglghkgBhvhCAQ0EIBYeRWFzeS1SU0EgR2VuZXJhdGVkIENlcnRpZmljYXRl\nMB0GA1UdDgQWBBTDr4BCNSdOEh+Lx6+4RRK11x8XcDCB1QYDVR0jBIHNMIHKgBRZ\n4KGhnll1W+K/KJVFl/C2+KM+JqGBpqSBozCBoDELMAkGA1UEBhMCQ0gxDzANBgNV\nBAgTBlp1cmljaDEPMA0GA1UEBxMGWnVyaWNoMRQwEgYDVQQKEwt2cG5ib29rLmNv\nbTELMAkGA1UECxMCSVQxFDASBgNVBAMTC3ZwbmJvb2suY29tMRQwEgYDVQQpEwt2\ncG5ib29rLmNvbTEgMB4GCSqGSIb3DQEJARYRYWRtaW5AdnBuYm9vay5jb22CCQCk\nbaY7CL3pNTATBgNVHSUEDDAKBggrBgEFBQcDAjALBgNVHQ8EBAMCB4AwDQYJKoZI\nhvcNAQEFBQADgYEAoDgD8mpVPnHUh7RhQziwhp8APC8K3jToZ0Dv4MYXQnzyXziH\nQbewJZABCcOKYS0VRB/6zYX/9dIBogA/ieLgLrXESIeOp1SfP3xt+gGXSiJaohyA\n/NLsTi/Am8OP211IFLyDLvPqZuqlh/+/GOLcMCeCrMj4RYxWstNxtguGQFc=\n-----END CERTIFICATE-----\n</cert>\n<key>\n-----BEGIN RSA PRIVATE KEY-----\nMIICXAIBAAKBgQCkTM/8E+JHCjskqMIwgYDrNCBTWZLa+qKkJjZ/rliJomTfVYwK\nwv1AHYYU6RHpCxS1qFp3BEKLvQlASuzycSv1FGnNiLmg94fqzzWdmjs1XWosnLqb\nOwxx2Ye/1WoakSHia0pItoZkxK7/fllm42+Qujri/ERGga5Cb/TfiP6pUQIDAQAB\nAoGANX508WQf9nVUUFlJ8LUZnnr4U2sEr5uPPNbcQ7ImTZm8MiMOV6qo/ikesMw5\n8qCS+5p26e1PJWRFENPUVhOW9c07z+nRMyHBQzFnNAFD7TiayjNk1gz1oIXarceR\nedNGFDdWCwXh+nJJ6whbQn9ioyTg9aqScrcATmHQxTit0GECQQDR5FmwC7g0eGwZ\nVHgSc/bZzo0q3VjNGakrA2zSXWUWrE0ybBm2wJNBYKAeskzWxoc6/gJa8mKEU+Vv\nugGb+J/tAkEAyGSEmWROUf4WX5DLl6nkjShdyv4LAQpByhiwLjmiZL7F4/irY4fo\nct2Ii5uMzwERRvHjJ7yzJJic8gkEca2adQJABxjZj4JV8DBCN3kLtlQFfMfnLhPd\n9NFxTusGuvY9fM7GrXXKSMuqLwO9ZkxRHNIJsIz2N20Kt76+e1CmzUdS4QJAVvbQ\nWKUgHBMRcI2s3PecuOmQspxG+D+UR3kpVBYs9F2aEZIEBuCfLuIW9Mcfd2I2NjyY\n4NDSSYp1adAh/pdhVQJBANDrlnodYDu6A+a4YO9otjd+296/T8JpePI/KNxk7N0A\ngm7SAhk379I6hr5NXdBbvTedlb1ULrhWV8lpwZ9HW2k=\n-----END RSA PRIVATE KEY-----\n</key>";
        private static readonly string _vpnBookConfigFileLocation = @"C:\Dispel\DispelRepos\OpenVPNClientApiCSharp\OpenVPNClientAPI_UnitTest\VpnBookConfigs\vpnbook-us2-tcp80.ovpn";
        private static readonly string _vpnBookUsername = "vpnbook";
        private static readonly string _vpnBookPassword = "Y6WtuUG";

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
