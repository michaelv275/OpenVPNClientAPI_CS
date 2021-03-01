using System;
using System.IO;
using System.Threading.Tasks;
using OpenVpnClientApi_CS;

namespace OpenVPNClientAPI_ConsoleAppTest
{
    class SingleConnection_Example
    {
        static readonly string _vpnConfig = "client\ndev tun\nproto udp\ntun-ipv6\nremote 134.209.116.42 1194\nredirect-gateway ipv6\nresolv-retry infinite\nnobind\nuser nobody\ngroup nogroup\npersist-key\npersist-tun\n<ca>\n-----BEGIN CERTIFICATE-----\nMIIGuTCCBKGgAwIBAgIUAlvXDUVQHTMCvTVIN+gKdyXB3QcwDQYJKoZIhvcNAQEL\nBQAwgZExCzAJBgNVBAYTAlVTMQswCQYDVQQIEwJXVjEXMBUGA1UEBxMOQmFyYmVx\ndWVkdmlsbGUxHTAbBgNVBAoTFFNpcm9jY29zIE1pbGRyZWQgSW5jMR0wGwYDVQQL\nExRTaXJvY2NvcyBNaWxkcmVkIEluYzEeMBwGA1UEAxMVQ2VydGlmaWNhdGUgQXV0\naG9yaXR5MB4XDTIxMDIwMzE5NDAwM1oXDTI0MDIwMzE5NDAwM1owgZExCzAJBgNV\nBAYTAlVTMQswCQYDVQQIEwJXVjEXMBUGA1UEBxMOQmFyYmVxdWVkdmlsbGUxHTAb\nBgNVBAoTFFNpcm9jY29zIE1pbGRyZWQgSW5jMR0wGwYDVQQLExRTaXJvY2NvcyBN\naWxkcmVkIEluYzEeMBwGA1UEAxMVQ2VydGlmaWNhdGUgQXV0aG9yaXR5MIICIjAN\nBgkqhkiG9w0BAQEFAAOCAg8AMIICCgKCAgEAxtX0L8tMMTHld8G2MHrzXpOlgB/g\nyj7vtYhZCLGAc4C+3prnjnXp0Ykyl/bftgs4RRcY4fDzt0yi3rSZWNwMWbtpRfnC\nyDkWdVK2dOFl+2sC7ejgPOhRpqbVdeSvcZs2dQdR+1vdfkvXcOhBZgupcmhXIKEb\ncjz+6Za7f/8La0ljj+8gGWugcjpZ4fJ/F1k2cn+7qn/mIFYuglzVS+WWnAJYdvNg\nv/XwP2+JPeT1dNYL9TqvlYPxyM12zSrmzyksZjhHw1RJYWxf7cYGJYfwvx1/b6p0\n4C+EbMArF1X99MGlc1e1MqrKrHAfm9iG2br93+Cm8FjxL1EGh362w5jt3iOk3aSy\nrMDmWlyjVDwhLYcAndhmE0Yem0u91qsAdwPNlg6EFos0u9Li0bULxb9Bya25Qipf\nBgoAvqtNOYeu8ren1INCC2Bq6h23PaAOKjtvoUJ3v6zbKF2ELZLESEhRMV1AV7ja\n5rf/YaZAN6yrsDeZxaZXGFJyZ0QdnMtgfBenCHkuCEjviiFFEjSqBb9JSMZCVwgN\nSn/yC7+mPEzlxBvX7cZbo8bVQR1Ou/bzV7DeeJOUfwRxpDrb57HM9xR3jG2WEjkp\n7UkeIwu8ddJuSpcekwSPqq/7KIKEFv74sQ0ypdAqzLui6lKLJqpWId3kgJAbivMV\n1fF1GevbQVVmutUCAwEAAaOCAQUwggEBMB0GA1UdDgQWBBQeyayNjqGj0QRmQy5z\nIyo5aXrr6DCB0QYDVR0jBIHJMIHGgBQeyayNjqGj0QRmQy5zIyo5aXrr6KGBl6SB\nlDCBkTELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAldWMRcwFQYDVQQHEw5CYXJiZXF1\nZWR2aWxsZTEdMBsGA1UEChMUU2lyb2Njb3MgTWlsZHJlZCBJbmMxHTAbBgNVBAsT\nFFNpcm9jY29zIE1pbGRyZWQgSW5jMR4wHAYDVQQDExVDZXJ0aWZpY2F0ZSBBdXRo\nb3JpdHmCFAJb1w1FUB0zAr01SDfoCnclwd0HMAwGA1UdEwQFMAMBAf8wDQYJKoZI\nhvcNAQELBQADggIBAMM0gIKQi52mpmfzChHvgjsac0ZinD+Px/F/7FW0NWjunqHM\npPc+LBTozk5z2OGEKNzMdNiMNiI+7+PDrZh9zeuq3FE6X3QFyh7sHIrTviMeH5Cx\n2Mu/fTbJlJt4pdUveZTUXMoERHLK0xSyM+KjWyXUdHSC8dJZIJql7mwb6RKWxr6M\nPfhcHzfk7VmlN+rUYQFqe1SWxlaqkWZP1ZquTQ17uVQh0tayxh9c+517nVF7zpZu\nWX1HBuTy5Ff3ik4+3RgCrKIXNtnK/BaSDDAM+1g79a10SwRsZ2/vanZ2/zC68g8a\nJ0G6szwnsHCXasdTcSECWCVRxBnmfyosD5m0rxVH1KW+wz+zTcsoov74ow5i0CZU\nw9zb8LoXP23D17rndSd/astJ23V9OcpOL/3IyncT8Iyayc1oxrUfyw5O5gJGK49w\nuWCsSNOswVPCTdq2lfW+/7bdQXv9+dfGOfKlUgLnlIX3Ee+JskcJ55z6kbPBPI+8\n6jj53ApZWi0wP0FCsSQzdIchNJoftml2vwlEwtZ6xb9EYvUP+Dx383SxV88ilYJ1\nyN1LuRrky1/tOKrTIaqxGzsWAD4K6z33j9KBzBxonrfbSb2oqC+WeO00XACxtiui\nE2/hFYwaGqDRT8SCWu6aen22ZGZ169fMfvdheQugQ+Le4LsxCzhFCOhO7U3t\n-----END CERTIFICATE-----\n\n</ca>\n<cert>\n-----BEGIN CERTIFICATE-----\nMIIG+TCCBOGgAwIBAgIFAJh+ewYwDQYJKoZIhvcNAQELBQAwgZExCzAJBgNVBAYT\nAlVTMQswCQYDVQQIEwJXVjEXMBUGA1UEBxMOQmFyYmVxdWVkdmlsbGUxHTAbBgNV\nBAoTFFNpcm9jY29zIE1pbGRyZWQgSW5jMR0wGwYDVQQLExRTaXJvY2NvcyBNaWxk\ncmVkIEluYzEeMBwGA1UEAxMVQ2VydGlmaWNhdGUgQXV0aG9yaXR5MB4XDTE5MTAx\nMjAwMDAwMFoXDTIzMTAxMjEyMDAwMFowgZIxCzAJBgNVBAYTAlVTMQswCQYDVQQI\nEwJOQzEWMBQGA1UEBxMNRW1wZXJvcnN2aWxsZTEgMB4GA1UEChMXRGVzZWdyZWdh\ndGlvbnMgU2VhbCBJbmMxIDAeBgNVBAsTF0Rlc2VncmVnYXRpb25zIFNlYWwgSW5j\nMRowGAYDVQQDExE2ODYwNzY0QkFEMkIxNDI5MDCCAiIwDQYJKoZIhvcNAQEBBQAD\nggIPADCCAgoCggIBAL1rwsHDr8KfTgdiEf8gSf8EOp18TiIILsvaGiGNBrgtGDXv\n8xCKBygNebrYFqhMLbqYQFlav1V+5KTRdZfqmHSiU/oztqJ0V0p6FBB28fgK/5FP\nmqsW1g8WgmH4rCj1c6Wv8uVe5Kw/Kg61KQ2XYXY87ZqotY+iKFnBrmWxDHKFbwvh\nDygyyCHy7HkCvkwCDjIAThQHfPZ5P4Xkth2CerrpvUMgaJor039JmHm13aEKkTcw\nQPm8UWZLnScdqwxMM5ny5sGN1WUWPdN0twDLWiwPAKdOvv9hgKI/ypqW611c2n8t\nkkdvDjNBgjjKHB3dulvm7idShTMALCT74llsP2ZB8DVktRxI0g1vd3tVg/zvQjED\ngiew3b3O0bgLjsuPrciuUlQ+OQpGdXHhVyzMsgCs1GvTV1uaTk0o/fwxpbvPok62\nt4bY1taQ3Xe4Kd/wsIJjixRuqDFnfoCHEvoS5kY22REHTzVfmjlMIN5ZqfqICJZY\nPfQKmNOJ+qNSA3oi8nQc+Mz5ZCzHuedxz6kLm62vYq6HkJIV1KAepbJSDYI+Z5Nx\nz1LSj3ZEhgcQIXf3ZM0ylxgMVJRzq/PYb8FRZ0xxqR4HyCcywMRfjwUeIdCjKXAm\nlWALTH2n3CrFaBJgUgeO3KkAYZIR38ri45cVrT+nKBmlPYwzTYqFpn3J4jlNAgMB\nAAGjggFTMIIBTzAJBgNVHRMEAjAAMC0GCWCGSAGG+EIBDQQgFh5FYXN5LVJTQSBH\nZW5lcmF0ZWQgQ2VydGlmaWNhdGUwHQYDVR0OBBYEFOop5soldexgF2XSP0HthT07\nUhEQMIHRBgNVHSMEgckwgcaAFB7JrI2OoaPRBGZDLnMjKjlpeuvooYGXpIGUMIGR\nMQswCQYDVQQGEwJVUzELMAkGA1UECBMCV1YxFzAVBgNVBAcTDkJhcmJlcXVlZHZp\nbGxlMR0wGwYDVQQKExRTaXJvY2NvcyBNaWxkcmVkIEluYzEdMBsGA1UECxMUU2ly\nb2Njb3MgTWlsZHJlZCBJbmMxHjAcBgNVBAMTFUNlcnRpZmljYXRlIEF1dGhvcml0\neYIUAlvXDUVQHTMCvTVIN+gKdyXB3QcwEwYDVR0lBAwwCgYIKwYBBQUHAwIwCwYD\nVR0PBAQDAgeAMA0GCSqGSIb3DQEBCwUAA4ICAQCr0sPfXKvW4djXjDp04JPvu9tH\nFLDQvugbD+WgfOqXdb+HcZvwTNY9eK4KmkJwH/yNkMLyVzyHLtjAQ5hvyysaVdkS\nqyDwb/bjDCJ2Bsi/roCRib5AzeBrgLej/IKwpRyl75w9MIo60JitNLyci41fV7Mv\na9gcr3J4Bpphk1Gb8XH/EDa2gvvh0FcVQ+C/yxlTgDDu/XVv8UAjRbmH07mAfDS5\nCtuD6je6npclfEoLV1PMBURwZqCtvTUKuX87YJ7jz4XoztB18tuL8/dTHNfO57k1\nJuRCMAjLMQ3mBPCc/shqvqsGIhDNQke40Zh6Mt+MwJ+MCFotWmHeNwsGqjcZyyrT\n29XNYEf3avxXYfKpJjoVrKS8UJlaTTjQhKbQ0yuzTRqkX88sf0QHloxWfPd53+xI\nGdsmQ6UF36bumWknd54eGc9h5jKSYZVx/WWlgaW9O5WKsSgCqU0ygjf8+r1AzpLo\nk98Wq5mwDJPAIjOblGI7uB7oyzZxLZy9R2WHvApMlEclOkq+urhzDr8M/s9AT5lp\n5ikTXYwz2cIcriZ5ezGyJ69lQyKD54X5RdVQKn2EaJhUQkATWWQjZFePdZ45EzQI\nXWcbU4mXZkc5SUrUCOCpaspINzqYghdXBhS9LNebgz90hL5Z5Z5Kxc7qnST2PAVj\niqDT3Ci7asNS2Q7ENQ==\n-----END CERTIFICATE-----\n\n</cert>\n<key>\n-----BEGIN PRIVATE KEY-----\n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\nJ2u4ZokzoJ78WCxoKEJ1uiJzer5Q\n-----END PRIVATE KEY-----\n\n</key>\nns-cert-type server\ncomp-lzo\nverb 3\ndhcp-option DNS 8.8.8.8\ndhcp-option DNS 8.8.4.4\ncipher AES-256-CBC\ntls-version-min 1.2\nauth SHA256\ntun-mtu 1500\nmssfix 1420\n<tls-auth>\n-----BEGIN OpenVPN Static key V1-----\na00f3bae305acc8b3fcb34bde55956ed\n66d6b86185014abaa0a0bdceca398477\nc8cbabd6071d5a33e809cabc0d7bb941\n07fcb6a874506bc34e23b9bf6390e167\nb8b7856bac5cf54cbbe6f1ecd858d8e1\n6b6d6a8f0b105916292a03fc6925ead2\neea42edb39649448662cea668ed7e141\nba8f7e2331822c67ee0100c5b9fa9d3b\n94931da1608f136c9c801d7960367d8f\n82cb161d7cbede8d381718271a048569\n5435cf18e007601392889c81d2ca9f14\n7ad7d0ae55f8c9f484169452f58dc351\n82c6de64df2445f93e27c512a876f469\n2b62756cda972024692b49e51b1b125d\nb57ddbeb10287fdb87d19e3f287c51f5\n0bf415c6297bb5830571c59d28d63f0b\n-----END OpenVPN Static key V1-----\n</tls-auth>";
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
