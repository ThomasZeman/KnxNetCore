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

        //SEARCH_REQUEST 0x0201
        //SEARCH_RESPONSE 0x0202
        //DESCRIPTION_REQUEST 0x0203
        //DESCRIPTION_RESPONSE 0x0204
        //CONNECTION_REQUEST 0x0205
        //CONNECTION_RESPONSE 0x0206
        //CONNECTIONSTATE_REQUEST 0x0207
        //CONNECTIONSTATE_RESPONSE 0x0208
        //DISCONNECT_REQUEST 0x0209
        //DISCONNECT_RESPONSE 0x020A
        //TUNNEL_REQUEST 0x0420
        //TUNNEL_RESPONSE 0x0421
        //DEVICE_CONFIGURATION_REQUEST 0x0310
        //DEVICE_CONFIGURATION_ACK 0x0311
        //ROUTING_INDICATION 0x0530
        public ushort ServiceType { get; private set; }
        public ushort TotalLength { get; private set; }
    }
}