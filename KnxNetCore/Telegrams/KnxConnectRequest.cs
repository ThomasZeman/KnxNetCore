using System.Net;

namespace KnxNetCore.Telegrams
{
    internal class KnxConnectRequest : KnxTelegramPayload
    {
        public KnxConnectRequest(IPEndPoint ipEndPoint)
        {
            IpEndPoint = ipEndPoint;
        }

        public IPEndPoint IpEndPoint { get; private set; }

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

        public byte ChannelId { get; private set; }

        public byte Status { get; private set; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}