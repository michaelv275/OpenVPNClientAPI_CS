using OpenVpnClientApi_CS.Interfaces;
using System;
using OpenVpnClientApi_CS.Exceptions;
using System.Collections.Generic;
using System.IO;

namespace OpenVpnClientApi_CS
{
    public class Client : IEventReceiver
    {
        private OpenVPNClientThread _clientThread { get; set; }
        private ClientAPI_Config _configData { get; set; }
        private ClientAPI_ProvideCreds _configCreds { get; set; }
        private ClientAPI_EvalConfig _configEvaluator { get; set; }

        /// <summary>
        /// creates a new Client object and initializes the C++ implementation Object, a new OpenVPNClientThread object
        /// CLientAPI_Config object, and a ClientAPI_ProvideCreds object. 
        /// </summary>
        public Client()
        {
            LoadCore();

            _clientThread = new OpenVPNClientThread();
            _configData = new ClientAPI_Config();
            _configCreds = new ClientAPI_ProvideCreds();
        }

        /// <summary>
        /// Sets the ClientAPI_Config object with a string. The string must be using newline delimeters "\n, \r, \r\n"
        /// </summary>
        /// <param name="configAsMultiLineString"></param>
        public void SetConfigWithMultiLineString(string configAsMultiLineString)
        {
            // load/evaluate config
            _configData = new ClientAPI_Config();

            _configData.content = configAsMultiLineString;
            _configData.compressionMode = "yes";

            _configEvaluator = _clientThread.eval_config(_configData);

            if (_configEvaluator.error)
            {
                throw new ConfigError("OpenVPN config file parse error: " + _configEvaluator.message);
            }
        }

        /// <summary>
        /// Sets the ClientAPI_Config object with the contents of a file. 
        /// </summary>
        /// <param name="fileLocation">The fully qualified location of the desired file to read</param>
        public void SetConfigWithFile(string fileLocation)
        {
            string configFileContent = String.Empty;

            if (File.Exists(fileLocation))
            {
                using (FileStream stream = File.Open(fileLocation, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    string lastLine = string.Empty;

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (!reader.EndOfStream)
                        {
                            if (!String.IsNullOrEmpty(lastLine = reader.ReadLine()))
                            {
                                configFileContent += String.Format("{0}\n", lastLine);
                            }
                        }
                    }
                }

                if (!String.IsNullOrEmpty(configFileContent))
                {
                    SetConfigWithMultiLineString(configFileContent);
                }
            }
            else
            {
                throw new FileNotFoundException("Could not find the file specified", fileLocation);
            }
        }

        //provide credentials if VPN server requires them
        public void AddCredentials(string username, string password, bool useCredentials)
        {

            if (useCredentials)
            {
                if (!_configEvaluator.autologin)
                {
                    if (!String.IsNullOrEmpty(username))
                    {
                        _configCreds.username = username;
                        _configCreds.password = password;
                        _configCreds.replacePasswordWithSessionID = true;
                    }
                }
            }

            if (_configCreds != null)
            {
                _clientThread.provide_creds(_configCreds);
            }
            else
            {
                throw new CredsUnspecifiedError("No ClientAPI_ProvideCreds object exists. Which probably means the Client object was not initialized");
            }
        }

        public void Connect()
        {
            Console.WriteLine("Client.Connect()");

            // connect
            _clientThread.Connect(this);

            // wait for worker thread to exit
            _clientThread.WaitThreadLong();
        }

        public void Stop()
        {
            _clientThread.stop();
        }

        public void Show_stats()
        {
            int n = OpenVPNClientThread.stats_n();
            for (int i = 0; i < n; ++i)
            {
                String name = OpenVPNClientThread.stats_name(i);
                long value = _clientThread.stats_value(i);
                if (value > 0)
                {
                    Console.WriteLine("STAT: {0}={1}", name, value);
                }
            }
        }

        public void ConnectionFinished(ClientAPI_Status status)
        {
            Console.WriteLine("DONE ClientAPI_Status: err={0} msg='{1}'", status.error, status.message);
        }

        public void Event_(ClientAPI_Event apiEvent)
        {
            bool error = apiEvent.error;
            String name = apiEvent.name;
            string info = apiEvent.info;
            Console.WriteLine("EVENT: err={0} name={1} info='{2}'", error, name, info);
        }

        public void ExternalPkiCertRequest(ClientAPI_ExternalPKICertRequest req)
        {
            req.error = true;
            req.errorText = "cert request failed: external PKI not implemented";
        }

        public void ExternalPkiSignRequest(ClientAPI_ExternalPKISignRequest req)
        {
            req.error = true;
            req.errorText = "sign request failed: external PKI not implemented";
        }

        public void Log(ClientAPI_LogInfo loginfo)
        {
            string text = loginfo.text;
            Console.WriteLine("LOG: {0}", text);
        }

        public bool PauseOnConnectionTimeout()
        {
            return false;
        }

        public bool SocketProtect(int socket)
        {
            return false;
        }

        public ITunBuilder TunBuilderNew()
        {
            return null;
        }

        private void LoadCore()
        {
            // Load OpenVPN core (implements ClientAPI_OpenVPNClient) from shared library 
            ClientAPI_OpenVPNClient.init_process();

            //For testing the crypto library
            //string test = ClientAPI_OpenVPNClient.crypto_self_test();
            //Console.WriteLine("Crypto self test: {0}", test);
        }
    }
}
