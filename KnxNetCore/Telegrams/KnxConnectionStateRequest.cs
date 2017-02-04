using System.Net;

namespace KnxNetCore.Telegrams
{
    internal class KnxConnectionStateRequest : KnxTelegramPayload
    {
        public KnxConnectionStateRequest(byte channelId, IPEndPoint endPoint)
        {
            ChannelId = channelId;
            EndPoint = endPoint;
        }

        public byte ChannelId { get; private set; }

        public IPEndPoint EndPoint { get; private set; }


        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}