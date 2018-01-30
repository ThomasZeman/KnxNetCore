namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxConnectResponse : KnxTelegramPayload
    {
        public enum StatusCodes : byte
        {
            NoError = 0,
            ConnectionTypeError = 0x22,
            ConnectionTypeError2 = 0x23,
            NoMoreConnections = 0x24
        }

        public KnxConnectResponse(byte channelId, StatusCodes statusCode)
        {
            ChannelId = channelId;
            StatusCode = statusCode;
        }

        public byte ChannelId { get; }

        public StatusCodes StatusCode { get; }

        public override string ToString()
        {
            return $"KnxConnectResponse: ChannelId: {ChannelId}, Status: {StatusCode}";
        }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }
    }
}