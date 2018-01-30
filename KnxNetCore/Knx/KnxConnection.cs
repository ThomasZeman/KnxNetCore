using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using KnxNetCore.Knx.Telegrams;

namespace KnxNetCore.Knx
{
    public sealed class KnxConnection : IDisposable
    {
        public enum States
        {
            Disconnected,
            Connecting,
            Connected,
            Disconnecting
        }


        public States State { get; private set; }
        public byte ChannelId { get; private set; }
        public IPEndPoint LocalEndpoint { get; }
        public IPEndPoint RemoteEndpoint { get; }

        public event Action<KnxConnection, CemiFrame> KnxEventReceived;

        private readonly UdpClient _socket;
        private readonly ITrace _trace;
        private readonly Visitor _visitor;
        private byte _sequenceCounter;
        private bool _stopLoops;

        public KnxConnection(IPEndPoint localEndpoint, IPEndPoint remoteEndpoint, ITrace trace)
        {
            _trace = trace;
            _visitor = new Visitor(this);
            LocalEndpoint = localEndpoint;
            RemoteEndpoint = remoteEndpoint;
            State = States.Disconnected;
            _socket = new UdpClient(localEndpoint);
            Task.Run(ReceiverLoop).TraceOnException(_trace);
            Task.Run(HealthCheck).TraceOnException(_trace);
        }

        internal async Task ReceiverLoop()
        {
            _trace.Write(TraceSeverities.Info, "Started telegram receiver");
            for (; !_stopLoops;)
            {
                try
                {
                    var result = await _socket.ReceiveAsync();
                    var buffer = result.Buffer;
                    var telegramHeader = Serializer.ParseKnxTelegramHeader(buffer);
                    var telegram = Serializer.ParseKnxTelegram(telegramHeader, buffer);
                    _trace.Write(TraceSeverities.Debug, "Telegram received");
                    telegram.Accept(_visitor);
                }
                catch (Exception e)
                {
                    _trace.Write(TraceSeverities.Error, "Exception while receiving telegram: {0}", e);
                }
            }
        }

        internal async Task HealthCheck()
        {
            for (; !_stopLoops;)
            {
                switch (State)
                {
                    case States.Disconnected:
                        _trace.Write(TraceSeverities.Debug, "Disconnected. Sending connect request.");
                        await SendConnectRequest();
                        break;
                    case States.Connecting:
                        _trace.Write(TraceSeverities.Warning, "Failed to connect since last tick.");
                        State = States.Disconnected;
                        break;
                    case States.Connected:
                        await SendConnectionStateRequest();
                        break;
                    case States.Disconnecting:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }

        private async Task KnxTunnelingRequest(KnxTunnelingRequest knxTunnelingRequest)
        {
            try
            {
                await SendTunnelingAcknowledge(knxTunnelingRequest.SequenceNumber);
                OnKnxEventReceived(this,knxTunnelingRequest.CemiFrame);
            }
            catch (Exception e)
            {
                _trace.Write(TraceSeverities.Error, "Failed to send acknowledge: {0}", e);
            }
        }

        private void KnxDisconnectRequest()
        {
            State = States.Disconnected;
        }

        private void KnxConnectionStateResponse(KnxConnectionStateResponse knxConnectionStateResponse)
        {
            switch (State)
            {
                case States.Disconnected:
                case States.Connecting:
                case States.Disconnecting:
                    _trace.Write(TraceSeverities.Warning, "Discarding KnxConnectResponse because state is not 'Connecting'");
                    break;
                case States.Connected:
                    if (knxConnectionStateResponse.Status ==
                        Knx.Telegrams.KnxConnectionStateResponse.StatusCodes.NoError)
                    {
                        _trace.Write(TraceSeverities.Debug, "Received KnxConnectionStateResponse indicating no error");
                    }
                    else
                    {
                        _trace.Write(TraceSeverities.Error, "Received KnxConnectionStateResponse indicating error: {0}", knxConnectionStateResponse.Status);
                        State = States.Disconnected;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void KnxConnectResponse(KnxConnectResponse knxConnectResponse)
        {
            switch (State)
            {
                case States.Disconnected:
                case States.Connected:
                case States.Disconnecting:
                    _trace.Write(TraceSeverities.Warning,
                        "Discarding KnxConnectResponse because state is not 'Connecting'");
                    break;
                case States.Connecting:
                    if (knxConnectResponse.StatusCode == Knx.Telegrams.KnxConnectResponse.StatusCodes.NoError)
                    {
                        _trace.Write(TraceSeverities.Info, "Connection successfully established: {0}", knxConnectResponse.ToString());
                        State = States.Connected;
                        ChannelId = knxConnectResponse.ChannelId;
                    }
                    else
                    {
                        _trace.Write(TraceSeverities.Error, "Failed to establish connection: {0}", knxConnectResponse.ToString());
                        State = States.Disconnected;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal async Task SendConnectRequest()
        {
            var header = new KnxConnectRequest(LocalEndpoint);
            var ms = new MemoryStream();
            Serializer.Serialize(header, ms);
            _trace.Write(TraceSeverities.Debug, "Sending SendConnectRequest: {0}", header.ToString());
            State = States.Connecting;
            await Send(ms, _socket);
        }

        internal async Task SendConnectionStateRequest()
        {
            var telegram = new KnxConnectionStateRequest(ChannelId, LocalEndpoint);
            var ms = new MemoryStream();
            Serializer.Serialize(telegram, ms);
            _trace.Write(TraceSeverities.Debug, "Sending SendConnectionStateRequest: {0}", telegram.ToString());
            await Send(ms, _socket);
        }

        public async Task<byte> SendTunnelingRequest(CemiFrame cemiFrame)
        {
            var sequenceNumber = _sequenceCounter++;
            var telegram = new KnxTunnelingRequest(ChannelId, sequenceNumber, cemiFrame);
            var ms = new MemoryStream();
            Serializer.Serialize(telegram, ms);
            _trace.Write(TraceSeverities.Debug, "Sending SendTunnelingRequest: {0}", telegram.ToString());
            await Send(ms, _socket);
            return sequenceNumber;
        }

        internal async Task SendTunnelingAcknowledge(byte sequenceCounter)
        {
            var telegram = new KnxTunnelingAcknowledge(ChannelId, sequenceCounter);
            var ms = new MemoryStream();
            Serializer.Serialize(telegram, ms);
            _trace.Write(TraceSeverities.Debug, "Sending SendTunnelingAcknowledge: {0}", telegram.ToString());
            await Send(ms, _socket);
        }

        internal async Task Send(MemoryStream memoryStream, UdpClient udpClient)
        {
            var buf = new byte[memoryStream.Length];
            memoryStream.Seek(0, SeekOrigin.Begin);
            memoryStream.Read(buf, 0, buf.Length);
            await _socket.SendAsync(buf, buf.Length, RemoteEndpoint);
        }

        private void OnKnxEventReceived(KnxConnection arg1, CemiFrame arg2)
        {
            KnxEventReceived?.Invoke(arg1, arg2);
        }

        public void Dispose()
        {
            _socket.Dispose();
            _stopLoops = true;
        }

        private class Visitor : IKnxTelegramVisitor
        {
            private readonly KnxConnection _knxConnection;

            public Visitor(KnxConnection knxConnection)
            {
                _knxConnection = knxConnection;
            }

            public void Visit(KnxConnectResponse knxConnectResponse)
            {
                _knxConnection.KnxConnectResponse(knxConnectResponse);
            }

            public void Visit(KnxConnectionStateRequest knxConnectionStateRequest)
            {
                throw new NotImplementedException();
            }

            public void Visit(KnxConnectRequest knxConnectRequest)
            {
                throw new NotImplementedException();
            }

            public void Visit(KnxConnectionStateResponse knxConnectionStateResponse)
            {
                _knxConnection.KnxConnectionStateResponse(knxConnectionStateResponse);
            }

            public void Visit(KnxTunnelingRequest knxTunnelingRequest)
            {
                _knxConnection._trace.Write(TraceSeverities.Info, "Received TunnelRequest: {0}", knxTunnelingRequest);
                _knxConnection.KnxTunnelingRequest(knxTunnelingRequest).TraceOnException(_knxConnection._trace);
            }

            public void Visit(KnxTunnelingAcknowledge knxTunnelingAcknowledge)
            {
                _knxConnection._trace.Write(TraceSeverities.Debug, "Received TunnelAck: {0}", knxTunnelingAcknowledge);
            }

            public void Visit(KnxDisconnectRequest knxDisconnectRequest)
            {
                _knxConnection._trace.Write(TraceSeverities.Info, "Received disconnect request: {0}", knxDisconnectRequest);
                _knxConnection.KnxDisconnectRequest();
            }
        }
    }
}