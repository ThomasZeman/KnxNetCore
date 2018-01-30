namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxTunnelingRequest : KnxTelegramPayload
    {
        public KnxTunnelingRequest(byte channelId, byte sequenceNumber, CemiFrame cemiFrame)
        {
            ChannelId = channelId;
            SequenceNumber = sequenceNumber;
            CemiFrame = cemiFrame;
        }

        public byte ChannelId { get; }
        public byte SequenceNumber { get; }
        public CemiFrame CemiFrame { get; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }

        public override string ToString()
        {
            return $"ChannelId: {ChannelId}, SequenceNumber: {SequenceNumber}, CemiFrame: {CemiFrame}";
        }
    }
}