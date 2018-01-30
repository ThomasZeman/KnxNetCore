using System.Net;

namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxConnectionStateRequest : KnxTelegramPayload
    {
        public KnxConnectionStateRequest(byte channelId, IPEndPoint endPoint)
        {
            ChannelId = channelId;
            EndPoint = endPoint;
        }

        public byte ChannelId { get; }

        public IPEndPoint EndPoint { get; }


        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}