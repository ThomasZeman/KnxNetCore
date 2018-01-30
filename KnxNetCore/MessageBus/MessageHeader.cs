namespace KnxNetCore.MessageBus
{
    public class MessageHeader
    {
        public BusAddress SourceAddress { get; }
        public BusAddress DestinationAddress { get; }

        public MessageHeader(BusAddress sourceAddress, BusAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }
}