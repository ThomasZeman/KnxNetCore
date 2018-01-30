namespace KnxNetCore.Knx.Telegrams
{
    internal interface IKnxTelegramVisitor
    {
        void Visit(KnxConnectResponse knxConnectResponse);
        void Visit(KnxConnectionStateRequest knxConnectionStateRequest);
        void Visit(KnxConnectRequest knxConnectRequest);
        void Visit(KnxConnectionStateResponse knxConnectionStateResponse);
        void Visit(KnxTunnelingRequest knxTunnelingRequest);
        void Visit(KnxTunnelingAcknowledge knxTunnelingAcknowledge);
        void Visit(KnxDisconnectRequest knxDisconnectRequest);
    }
}