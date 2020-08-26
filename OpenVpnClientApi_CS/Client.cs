using OpenVpnClientApi_CS.Exceptions;
using OpenVpnClientApi_CS.Interfaces;
using System;
using System.IO;

namespace OpenVpnClientApi_CS
{
    public class Client : IEventReceiver
    {
        private OpenVPNClientThread _clientThread;
        private ClientAPI_Config _configData;
        private ClientAPI_ProvideCreds _configCreds;
        private ClientAPI_EvalConfig _configEvaluator;

        public event EventHandler ConnectionClosed;
        public event EventHandler ConnectionAttemptCompleted;
        public event EventHandler<ClientAPI_Event> CoreEventReceived;

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

            _clientThread = new OpenVPNClientThread() { Manager = this };
            _configData = new ClientAPI_Config();
            _configCreds = new ClientAPI_ProvideCreds();
        }

        /// <summary>
        /// Event handler for the ConnectionClosed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnConnectionClosed()
        {
            ConnectionClosed?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Event handler for the ConnectionClosed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnConnectionAttemptCompleted()
        {
            ConnectionAttemptCompleted?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Event handler for the CoreEventReceived event
        /// Default is to log the message to the console.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnCoreEventReceived(ClientAPI_Event message)
        {
            if (CoreEventReceived != null)
            {
                CoreEventReceived.Invoke(this, message);
            }
            else
            {
                Console.WriteLine("EVENT: err={0} name={1} info='{2}'", message.error, message.name, message.info);
            }
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
        /// Use the OpenVPNClientThread object to start a connection.
        /// 
        /// If another connection is active, end it. Once that connection is ended, start the new one.
        /// </summary>
        public void Connect()
        {
            if (ClientThread.IsCurrentlyRunning())
            {
                //Call Connect() after disconnection complete
                //ConnectionClosed += (o, e) => Connect();

                //close the previous connection
                Console.WriteLine("Closing previous connection");

                Stop();
            }
            else
            {
                //remove callback tothis method so we don't get stuck in an endless loop.
                ConnectionClosed -= (o, e) => Connect();

                ClientThread.Connect(this);
            }
        }

        /// <summary>
        /// Stop the OpenVPNClientThread
        /// </summary>
        public void Stop()
        {
            ClientThread.stop();

            ClientThread.WaitThreadShort(5);
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
        /// Writes a message to the console window, then triggers the ConnectionClosed Event
        /// </summary>
        /// <param name="status">The ClientAPI_Status object containing the data to write</param>
        public void ConnectionFinished(ClientAPI_Status status)
        {
            OnConnectionClosed();
        }

        /// <summary>
        /// Fired when connection is started, stopped, or cancelled
        /// Any event returned from the CoreLibrary goes through here.
        /// 
        /// If the event was a connected or disconnected event, the OnConnectionAttemptCompleted and/or OnConnectionClosed events will be fired
        /// </summary>
        /// <param name="apiEvent"></param>
        public void Event_(ClientAPI_Event apiEvent)
        {
            if (String.Equals(apiEvent.name, "Connected", StringComparison.OrdinalIgnoreCase))
            {
                OnConnectionAttemptCompleted();
            }
            else
            {
                OnCoreEventReceived(apiEvent);
            }
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
