namespace OpenVpnClientApi_CS.Interfaces
{
    internal interface IEventReceiver
    {
        // Called with events from core. Event is a reserved word, so it is renamed Event_
        void Event_(ClientAPI_Event apiEvent);

        // Called with log text from core
        void Log(ClientAPI_LogInfo loginfo);

        // Called when connect() thread exits
        void ConnectionFinished(ClientAPI_Status status);

        // Called to "protect" a socket from being routed through the tunnel
        bool SocketProtect(int socket);

        // When a connection is close to timeout, the core will call this
        // method.  If it returns false, the core will disconnect with a
        // CONNECTION_TIMEOUT event.  If true, the core will enter a PAUSE
        // state.
        bool PauseOnConnectionTimeout();

        // Callback to get a certificate
        void ExternalPkiCertRequest(ClientAPI_ExternalPKICertRequest req);

        // Callback to sign data
        void ExternalPkiSignRequest(ClientAPI_ExternalPKISignRequest req);
    }
}
