namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxConnectionStateResponse : KnxTelegramPayload
    {
        public enum StatusCodes : byte
        {
            NoError = 0,
            ConnectionId = 0x21,
            DataConnection = 0x26,
            KnxConnection = 0x27
        }

        public KnxConnectionStateResponse(byte channelId, StatusCodes status)
        {
            ChannelId = channelId;
            Status = status;
        }

        public byte ChannelId { get; }

        public StatusCodes Status { get; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}