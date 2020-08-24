using System;
using System.Threading;
using OpenVpnClientApi_CS.Interfaces;
using OpenVpnClientApi_CS.Exceptions;

namespace OpenVpnClientApi_CS
{
    public class OpenVPNClientThread : ClientAPI_OpenVPNClient
    {
        private IEventReceiver _parent;
        private ITunBuilder _tunnelBuilder;
        private Thread _clientThread;
        private ClientAPI_Status _apiConnectionStatus;
        private bool _hasConnectBeencalled = false;

        private int _bytesInIndex = -1;
        private int _bytesOutIndex = -1;

        public OpenVPNClientThread()
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

        //Start connect session in worker thread
        public void Connect(IEventReceiver parent_arg)
        {
            Console.WriteLine("OpenVPNClientThread.Connect(IEventReceiver parent_arg)");

            if (_hasConnectBeencalled)
            {
                throw new ConnectionCalledTwiceException();
            }

            _hasConnectBeencalled = true;

            // direct client callbacks to parent
            _parent = parent_arg;

            // clear status
            _apiConnectionStatus = null;

            // execute client in a worker thread
            _clientThread = new Thread(new ThreadStart(Run)) { Name = "OpenVPNClientThread" };
            _clientThread.Start();
        }

        // Wait for worker thread to complete; to stop thread,
        // first call super stop() method then wait_thread().
        // This method will give the thread one second to
        // exit and will abandon it after this time.
        public void WaitThreadShort()
        {
            int waitTimeMs = 5000; // max time that we will wait for thread to exit

            if (_clientThread != null)
            {
                try
                {
                    if (_clientThread.Join(waitTimeMs))
                    {
                        //client thread joined successfully. Log?
                    }
                }
                catch (Exception interruptedException)
                {
                    //This means it was joined. Consider logging?
                }

                // thread failed to stop?
                if (_clientThread.IsAlive)
                {
                    // abandon thread and deliver our own status object to instantiator.
                    ClientAPI_Status status = new ClientAPI_Status();
                    status.error = true;
                    status.message = ("CORE_THREAD_ABANDONED");

                    EndClientThread(status);
                }
            }
        }

        // Wait for worker thread to complete; to stop thread,
        // first call super stop() method then wait_thread().
        // This method will wait forever for the thread to exit.
        public void WaitThreadLong()
        {
            if (_clientThread != null)
            {
                bool interrupted = false;

                while (!interrupted)
                {
                    try
                    {
                        //will wait indefinitely. if it finishes without error, we still want to break out of the loop and stop the connection;
                        _clientThread.Join();

                        throw new Exception("Client thread joined successfully");
                    }
                    catch (Exception e)
                    {
                        interrupted = true;
                        base.stop(); // send thread a stop message
                    }
                }
            }
        }

        /// <summary>
        /// prints how many bytes were received with base.stats_value(index) in C++
        /// </summary>
        /// <returns></returns>
        public long GetBytesIn()
        {
            return base.stats_value(_bytesInIndex);
        }

        /// <summary>
        /// prints how many bytes were sent with base.stats_value(index) in C++
        /// </summary>
        /// <returns></returns>
        public long GetBytesOut()
        {
            return base.stats_value(_bytesOutIndex);
        }

        private void EndClientThread(ClientAPI_Status status)
        {
            Console.WriteLine("OpenVPNClientThread.EndClientThread(ClientAPI_Status status)");
            IEventReceiver parent = FinalizeThread(status);

            parent?.ConnectionFinished(_apiConnectionStatus);
        }

        private IEventReceiver FinalizeThread(ClientAPI_Status connect_status)
        {
            IEventReceiver finalizedParent = _parent;

            if (finalizedParent != null)
            {
                // save thread connection status
                _apiConnectionStatus = connect_status;

                // disassociate client callbacks from parent
                _parent = null;
                _tunnelBuilder = null;
                _clientThread = null;
            }

            return finalizedParent;
        }

        public void Run()
        {
            Console.WriteLine("OpenVPNClientThread.Run()");

            // Call out to core to start connection.
            // Doesn't return until connection has terminated.
            ClientAPI_Status status = base.connect();

            EndClientThread(status);
        }

        #region ClientAPI_OpenVPNClient (C++ class) overrides

        public bool SocketProtect(int socket)
        {
            bool isSocketProtected = false;

            if (_parent != null)
            {
                isSocketProtected = _parent.SocketProtect(socket);
            }

            return isSocketProtected;
        }

        public override bool pause_on_connection_timeout()
        {
            bool isPaused = false;

            if (_parent != null)
            {
                isPaused = _parent.PauseOnConnectionTimeout();
            }

            return isPaused;
        }

        public override void event_(ClientAPI_Event apiEvent)
        {
            _parent?.Event_(apiEvent);
        }

        public override void log(ClientAPI_LogInfo loginfo)
        {
            _parent?.Log(loginfo);
        }

        public override void external_pki_cert_request(ClientAPI_ExternalPKICertRequest req)
        {
            _parent?.ExternalPkiCertRequest(req);
        }

        public override void external_pki_sign_request(ClientAPI_ExternalPKISignRequest req)
        {
            _parent?.ExternalPkiSignRequest(req);
        }
        #endregion

        #region TunBuilderBase (C++ class) overrides

        public override bool tun_builder_new()
        {
            if (_parent != null)
            {
                _tunnelBuilder = _parent.TunBuilderNew();
            }

            return _tunnelBuilder != null;
        }


        public override bool tun_builder_set_remote_address(string address, bool ipv6)
        {
            bool isRemoteAddressSet = false;

            if (_tunnelBuilder != null)
            {
                isRemoteAddressSet = _tunnelBuilder.TunBuilderSetRemoteAddress(address, ipv6);
            }

            return isRemoteAddressSet;
        }


        public override bool tun_builder_add_address(string address, int prefix_length, string gateway, bool ipv6, bool net30)
        {
            bool isAddressSet = false;

            if (_tunnelBuilder != null)
            {
                isAddressSet = _tunnelBuilder.TunBuilderAddAddress(address, prefix_length, gateway, ipv6, net30);
            }

            return isAddressSet;
        }


        public bool tun_builder_reroute_gw(bool ipv4, bool ipv6, long flags)
        {
            bool isGatewayRerouted = false;

            if (_tunnelBuilder != null)
            {
                isGatewayRerouted = _tunnelBuilder.TunBuilderRerouteGw(ipv4, ipv6, flags);
            }
            
            return isGatewayRerouted;
        }


        public override bool tun_builder_add_route(string address, int prefix_length, int metric, bool ipv6)
        {
            bool isRouteAdded = false;

            if (_tunnelBuilder != null)
            {
                return _tunnelBuilder.TunBuilderAddRoute(address, prefix_length, ipv6);
            }

            return isRouteAdded;
        }


        public override bool tun_builder_exclude_route(string address, int prefix_length, int metric, bool ipv6)
        {
            bool isRouteExcluded = false;

            if (_tunnelBuilder != null)
            {
                isRouteExcluded = _tunnelBuilder.TunBuilderExcludeRoute(address, prefix_length, ipv6);
            }
            
            return isRouteExcluded;
        }


        public override bool tun_builder_add_dns_server(string address, bool ipv6)
        {
            bool isDNSServerSet = false;

            if (_tunnelBuilder != null)
            {
                isDNSServerSet = _tunnelBuilder.TunBuilderAddDnsServer(address, ipv6);
            }
            
                return isDNSServerSet;
        }


        public override bool tun_builder_add_search_domain(string domain)
        {
            bool isSearchDomainAdded = false;

            if (_tunnelBuilder != null)
            {
                isSearchDomainAdded = _tunnelBuilder.TunBuilderAddSearchDomain(domain);
            }
            
            return isSearchDomainAdded;
        }


        public override bool tun_builder_set_mtu(int mtu)
        {
            bool isMtuSet = false;

            if (_tunnelBuilder != null)
            {
                isMtuSet = _tunnelBuilder.TunBuilderSetMtu(mtu);
            }

            return isMtuSet;
        }


        public override bool tun_builder_set_session_name(string name)
        {
            bool isSessionNameSet = false;

            if (_tunnelBuilder != null)
            {
                isSessionNameSet = _tunnelBuilder.TunBuilderSetSessionName(name);
            }

            return isSessionNameSet;
        }

        public override int tun_builder_establish()
        {
            int tunnelPort = -1;

            if (_tunnelBuilder != null)
            {
                tunnelPort = _tunnelBuilder.TunBuilderEstablish();
            }

            return tunnelPort;
        }


        public override void tun_builder_teardown(bool disconnect)
        {
            if (_tunnelBuilder != null)
            {
                _tunnelBuilder.TunBuilderTeardown(disconnect);
            }
        }
        #endregion
    }
}
