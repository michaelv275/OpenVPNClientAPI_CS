using OpenVpnClientApi_CS.Exceptions;
using OpenVpnClientApi_CS.Interfaces;
using System;
using System.IO;

namespace OpenVpnClientApi_CS
{
    /// <summary>
    /// ***ELEVATED PRIVILEGES ARE REQUIRED*** to make changes to the routing table in Windows 10. Ensure your app is eleavted before calling any methods in this API.
    /// 
    /// This class is the entry point to use the API in C#. It also serves as the event receiver, so all openvpn events will be caught using the public events here. 
    /// By default, the events will be written to a console window. It is up to the implementer of the API to handle the events they wish.
    /// All methods and properties in this class exist solely in .NET and do not interact directly with the openVPN3 library. 
    /// Instead, the methods in this class will utilize the OpenVPNClientThread.cs object to interact with the underlying C++ openVPN3 library.
    /// </summary>
    public class Client : IEventReceiver
    {
        private OpenVPNClientThread _clientThread;
        private ClientAPI_Config _configData;
        private ClientAPI_ProvideCreds _configCreds;
        private ClientAPI_EvalConfig _configEvaluator;

        #region Public events
        /// <summary>
        /// Event handler for the ConnectionClosed event
        /// Default is to write "Disconnected from VPN" to the console. 
        /// </summary>
        public event EventHandler ConnectionClosed;

        /// <summary>
        /// Event handler for the ConnectionClosed event
        /// Default is to write "Connected to VPN" to the console.
        /// </summary>
        public event EventHandler ConnectionEstablished;

        /// <summary>
        /// Event handler for the ConnectionTimedOut event
        /// Default is to write "Connection request timed out" to the console
        /// </summary>
        public event EventHandler ConnectionTimedOut;

        /// <summary>
        /// Event handler for the Log and Stat events
        /// Default is to log the message to the console.
        /// </summary>
        public event EventHandler<string> LogReceived;

        /// <summary>
        /// Event handler for the CoreEventReceived event
        /// Default is to log the message to the console.
        /// </summary>
        public event EventHandler<ClientAPI_Event> CoreEventReceived;
        #endregion

        /// <summary>
        /// creates a new Client object and initializes the C++ implementation Object, a new OpenVPNClientThread object
        /// CLientAPI_Config object, and a ClientAPI_ProvideCreds object. 
        /// </summary>
        public Client()
        {
            SetDefaultManagementComponents();
        }

        private void OnConnectionClosed()
        {
            if (ConnectionClosed != null)
            {
                ConnectionClosed.Invoke(this, new EventArgs());
            }
            else
            {
                Console.WriteLine("Disconnected from VPN");
            }
        }

        private void OnConnectionEstablished()
        {
            if (ConnectionEstablished != null)
            {
                ConnectionEstablished.Invoke(this, new EventArgs());
            }
            else
            {
                Console.WriteLine("Connected to VPN");
            }
        }

        private void OnCoreEventReceived(ClientAPI_Event message)
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

        private void OnLogReceived(string logMessage)
        {
            if (CoreEventReceived != null)
            {
                LogReceived.Invoke(this, logMessage);
            }
            else
            {
                Console.WriteLine(logMessage);
            }
        }

        private void OnConnectionTimedOut()
        {
            if (CoreEventReceived != null)
            {
                ConnectionTimedOut.Invoke(this, new EventArgs());
            }
            else
            {
                Console.WriteLine("The connection request has timed out");
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
            //TODO add option?
            //_configData.tunPersist = false;

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
                credentialStatus = _clientThread.provide_creds(_configCreds);
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
            if (_clientThread.IsCurrentlyRunning())
            {
                string errorMessage = "Before starting another connection, the current client object must be stopped (clientObj.Stop()) ";
                errorMessage += " Then, the object's config and credentials must be reset with the new values, then Connect() can be called";

                throw new ConnectionCalledTwiceException(errorMessage);
            }
            else
            {
                _clientThread.Connect(this);
            }
        }

        /// <summary>
        /// Stop the OpenVPNClientThread
        /// </summary>
        public void Stop()
        {
            _clientThread.Stop();
        }

        /// <summary>
        /// Checks if the VPN thread is not null and is alive
        /// </summary>
        /// <returns>Whether the VPN thread is currently in use</returns>
        public bool IsVPNActive()
        {
            return _clientThread.IsCurrentlyRunning();
        }

        /// <summary>
        /// Gets the IO stats from the openVPN core library and fires the OnLogReceived event with output
        /// </summary>
        public void Show_stats()
        {
            int statCount = OpenVPNClientThread.stats_n();
            bool printHeader = true;

            for (int i = 0; i < statCount; ++i)
            {
                string name = OpenVPNClientThread.stats_name(i);
                long value = _clientThread.stats_value(i);

                if (value > 0)
                {
                    if (printHeader)
                    {
                        printHeader = false;

                        OnLogReceived("STATISTICS:");
                    }

                    OnLogReceived(String.Format("STAT: {0}={1}", name, value));
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
            //Release all connection data
            SetDefaultManagementComponents();

            OnConnectionClosed();
        }

        /// <summary>
        /// Fired when connection is started, stopped, or cancelled
        /// Any event returned from the CoreLibrary goes through here.
        /// 
        /// If the event was a connected event, the OnConnectionAttemptCompleted event will be fired
        /// </summary>
        /// <param name="apiEvent"></param>
        public void Event_(ClientAPI_Event apiEvent)
        {
            if (String.Equals(apiEvent.name, "Connected", StringComparison.OrdinalIgnoreCase))
            {
                OnConnectionEstablished();
            }
            else
            {
                OnCoreEventReceived(apiEvent);
            }
        }

        /// <summary>
        /// Callback to get a certificate
        /// </summary>
        /// <param name="req"></param>
        public void ExternalPkiCertRequest(ClientAPI_ExternalPKICertRequest req)
        {
            req.error = true;
            req.errorText = "cert request failed: external PKI not implemented";
        }

        /// <summary>
        /// Callback to sign data
        /// </summary>
        /// <param name="req"></param>
        public void ExternalPkiSignRequest(ClientAPI_ExternalPKISignRequest req)
        {
            req.error = true;
            req.errorText = "sign request failed: external PKI not implemented";
        }

        /// <summary>
        /// receives a message from the OpenVPN core library and fires OnLogReceived event
        /// </summary>
        /// <param name="loginfo"></param>
        public void Log(ClientAPI_LogInfo loginfo)
        {
            string text = String.Format("LOG: {0}", loginfo.text);

            OnLogReceived(text);
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
            bool shouldPause = false;

            if (!shouldPause)
            {
                OnConnectionTimedOut();
            }
            
            return shouldPause;
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

        private void SetDefaultManagementComponents()
        {
            _clientThread = new OpenVPNClientThread() { Manager = this };
            _configData = new ClientAPI_Config();
            _configCreds = new ClientAPI_ProvideCreds();
            _configEvaluator = null;
        }
    }
}
