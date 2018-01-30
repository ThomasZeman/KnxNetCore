namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxTunnelingAcknowledge : KnxTelegramPayload
    {
        public KnxTunnelingAcknowledge(byte channelId, byte sequenceNumber)
        {
            ChannelId = channelId;
            SequenceNumber = sequenceNumber;
        }

        public byte ChannelId { get; }
        public byte SequenceNumber { get; }

        public override void Accept(IKnxTelegramVisitor knxTelegramVisitor)
        {
            knxTelegramVisitor.Visit(this);
        }

        public override string ToString()
        {
            return $"ChannelId: {ChannelId}, SequenceNumber: {SequenceNumber}";
        }
    }
}