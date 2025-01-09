﻿using OpenVpnClientApi_CS.Exceptions;
using OpenVpnClientApi_CS.Interfaces;
using System.Threading;

namespace OpenVpnClientApi_CS
{
    /// <summary>
    /// This class interacts directly with the underlying C++ openVPN3 library. Rather, the code that was generated by swig, that created invocable
    /// C# methods from the C++ code.
    /// </summary>
    internal class OpenVPNClientThread : ClientAPI_OpenVPNClient
    {
        private Thread _clientThread;
        private ClientAPI_Status _apiConnectionStatus;
        private bool _hasConnectBeencalled = false;

        private int _bytesInIndex = -1;
        private int _bytesOutIndex = -1;

        //Logging that is not done through the core (C++), must be done through this manager
        internal Client Manager { get; set; }

        internal OpenVPNClientThread()
        {
            int statCount = stats_n();

            for (int i = 0; i < statCount; ++i)
            {
                string name = stats_name(i);

                if (name == "BYTES_IN")
                {
                    _bytesInIndex = i;
                }
                else if (name == "BYTES_OUT")
                {
                    _bytesOutIndex = i;
                }
            }
        }

        ///Start connect session in worker thread
        internal void Connect(Client parent_arg)
        {
            if (_hasConnectBeencalled)
            {
                string errorMessage = "Before starting another connection, the current client object must be stopped (clientObj.Stop()) ";
                errorMessage += " Then, the object's config and credentials must be reset with the new values, then Connect() can be called";

                throw new ConnectionCalledTwiceException(errorMessage);
            }

            _hasConnectBeencalled = true;

            // direct client callbacks to parent
            Manager = parent_arg;

            // clear status
            _apiConnectionStatus = null;

            // execute client in a worker thread
            _clientThread = new Thread(new ThreadStart(Run)) { Name = "OpenVPNClientThread" };
            _clientThread.Start();
        }

        /// <summary>
        /// prints how many bytes were received with base.stats_value(index) in C++
        /// </summary>
        /// <returns></returns>
        internal long GetBytesIn()
        {
            return base.stats_value(_bytesInIndex);
        }

        /// <summary>
        /// prints how many bytes were sent with base.stats_value(index) in C++
        /// </summary>
        /// <returns></returns>
        internal long GetBytesOut()
        {
            return base.stats_value(_bytesOutIndex);
        }

        internal void Run()
        {
            // Call out to core to start connection.
            // Doesn't return until connection has terminated.
            ClientAPI_Status status = base.connect();

            EndClientThread(status);
        }

        internal void Stop()
        {
            base.stop();
        }

        internal bool IsCurrentlyRunning()
        {
            return _clientThread != null && _clientThread.IsAlive;
        }

        /// <summary>
        /// Terminates _clientThread object and any work it was doing
        /// </summary>
        /// <param name="status"></param>
        private void EndClientThread(ClientAPI_Status status)
        {
            IEventReceiver parent = FinalizeThread(status);

            parent?.ConnectionFinished(_apiConnectionStatus);
            base.Dispose();

            if (!(parent is null))
            {
                //reset everything
                Manager = new Client();
            }
        }

        private IEventReceiver FinalizeThread(ClientAPI_Status connect_status)
        {
            IEventReceiver finalizedParent = Manager;

            if (finalizedParent != null)
            {
                // save thread connection status
                _apiConnectionStatus = connect_status;

                // disassociate client callbacks from parent
                Manager = null;
                _clientThread = null;
                _hasConnectBeencalled = false;
            }

            return finalizedParent;
        }

        #region ClientAPI_OpenVPNClient (C++ class) overrides

        internal bool SocketProtect(int socket)
        {
            bool isSocketProtected = false;

            if (Manager != null)
            {
                isSocketProtected = Manager.SocketProtect(socket);
            }

            return isSocketProtected;
        }

        public override bool pause_on_connection_timeout()
        {
            bool isPaused = false;

            if (Manager != null)
            {
                isPaused = Manager.PauseOnConnectionTimeout();
            }

            return isPaused;
        }

        public override void event_(ClientAPI_Event apiEvent)
        {
            Manager?.Event_(apiEvent);
        }

        public override void log(ClientAPI_LogInfo loginfo)
        {
            Manager?.Log(loginfo);
        }

        public override void external_pki_cert_request(ClientAPI_ExternalPKICertRequest req)
        {
            Manager?.ExternalPkiCertRequest(req);
        }

        public override void external_pki_sign_request(ClientAPI_ExternalPKISignRequest req)
        {
            Manager?.ExternalPkiSignRequest(req);
        }
        #endregion
    }
}
