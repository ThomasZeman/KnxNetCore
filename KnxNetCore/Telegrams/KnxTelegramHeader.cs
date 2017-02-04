namespace KnxNetCore.Telegrams
{
    internal class KnxTelegramHeader
    {
        public KnxTelegramHeader(byte headerLength, byte protocolVersion, ushort serviceType, ushort totalLength)
        {
            HeaderLength = headerLength;
            ProtocolVersion = protocolVersion;
            ServiceType = serviceType;
            TotalLength = totalLength;
        }

        public byte HeaderLength { get; private set; }
        public byte ProtocolVersion { get; private set; }
        public ushort ServiceType { get; private set; }
        public ushort TotalLength { get; private set; }
    }
}