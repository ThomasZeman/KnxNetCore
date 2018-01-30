using System.Net;

namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxConnectRequest : KnxTelegramPayload
    {
        public KnxConnectRequest(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
        }

        public IPEndPoint IpEndPoint { get; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }

    internal class KnxDisconnectRequest : KnxTelegramPayload
    {
        public KnxDisconnectRequest(byte channelId, byte status)
        {
            ChannelId = channelId;
            Status = status;
        }

        public byte ChannelId { get; }

        public byte Status { get; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}