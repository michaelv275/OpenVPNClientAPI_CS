using OpenVpnClientApi_CS.Interfaces;
using System;
using OpenVpnClientApi_CS.Exceptions;
using System.Collections.Generic;
using System.IO;

namespace OpenVpnClientApi_CS
{
    public class Client : IEventReceiver
    {
        private OpenVPNClientThread _clientThread;
        private ClientAPI_Config _configData;
        private ClientAPI_ProvideCreds _configCreds;
        private ClientAPI_EvalConfig _configEvaluator;

        public OpenVPNClientThread ClientThread { get => _clientThread; }
        public ClientAPI_Config ConfigData { get => _configData; }
        public ClientAPI_ProvideCreds ConfigCreds { get => _configCreds; }
        public ClientAPI_EvalConfig ConfigEvaluator { get => _configEvaluator; }

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

        /// <summary>
        /// provide credentials if VPN server requires them
        /// </summary>
        /// <param name="useCredentials"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        public ClientAPI_Status AddCredentials(bool useCredentials, string username = null, string password = null)
        {
            ClientAPI_Status credentialStatus = new ClientAPI_Status();

            if (useCredentials)
            {
                if (!ConfigEvaluator.autologin)
                {
                    if (!String.IsNullOrEmpty(username))
                    {
                        ConfigCreds.username = username;
                        ConfigCreds.password = password;
                        ConfigCreds.replacePasswordWithSessionID = true;
                    }
                }
            }

            if (_configCreds != null)
            {
                credentialStatus = ClientThread.provide_creds(_configCreds);
            }
            else
            {
                throw new CredsUnspecifiedError("No ClientAPI_ProvideCreds object exists. Which probably means the Client object was not initialized");
            }

            return credentialStatus;
        }

        /// <summary>
        /// Use the OpenVPNClientThread object to start a connection
        /// </summary>
        public void Connect()
        {
            // connect
            ClientThread.Connect(this);

            // wait for worker thread to exit
            ClientThread.WaitThreadLong();
        }

        /// <summary>
        /// Stop the OpenVPNClientThread
        /// </summary>
        public void Stop()
        {
            ClientThread.stop();
        }

        /// <summary>
        /// Prints the input/ouput stats to the console
        /// </summary>
        public void Show_stats()
        {
            int statCount = OpenVPNClientThread.stats_n();

            for (int i = 0; i < statCount; ++i)
            {
                string name = OpenVPNClientThread.stats_name(i);
                long value = ClientThread.stats_value(i);

                if (value > 0)
                {
                    Console.WriteLine("STAT: {0}={1}", name, value);
                }
            }
        }

        /// <summary>
        /// Called when the connection has been cancelled, or stopped.
        /// Writes a message to the console window
        /// </summary>
        /// <param name="status">The ClientAPI_Status object containing the data to write</param>
        public void ConnectionFinished(ClientAPI_Status status)
        {
            Console.WriteLine("DONE ClientAPI_Status: err={0} msg='{1}'", status.error, status.message);
        }

        /// <summary>
        /// Fired when connection is started, stopped, or cancelled
        /// Writes any errors to the output console
        /// </summary>
        /// <param name="apiEvent"></param>
        public void Event_(ClientAPI_Event apiEvent)
        {
            bool error = apiEvent.error;
            string name = apiEvent.name;
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

        /// <summary>
        /// Writes a message to the console
        /// </summary>
        /// <param name="loginfo"></param>
        public void Log(ClientAPI_LogInfo loginfo)
        {
            string text = loginfo.text;
            Console.WriteLine("LOG: {0}", text);
        }

        /// <summary>
        /// When a connection is close to timeout, the core will call this
        /// method.  If it returns false, the core will disconnect with a
        /// CONNECTION_TIMEOUT event.  If true, the core will enter a PAUSE
        /// state.
        /// 
        /// False by default
        /// </summary>
        /// <returns></returns>
        public bool PauseOnConnectionTimeout()
        {
            return false;
        }

        /// <summary>
        /// Called to "protect" a socket from being routed through the tunnel
        /// False by default
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public bool SocketProtect(int socket)
        {
            return false;
        }

        /// <summary>
        /// Creates a new instance of the ITunBuilder interface
        /// null by default
        /// </summary>
        /// <returns></returns>
        public ITunBuilder TunBuilderNew()
        {
            return null;
        }

        private void LoadCore()
        {
            // Load OpenVPN core (implements ClientAPI_OpenVPNClient) from shared library 
            ClientAPI_OpenVPNClient.init_process();
        }
    }
}
