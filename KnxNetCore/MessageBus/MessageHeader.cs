namespace KnxRadio
{
    public class MessageHeader
    {
        public IMessageBusAddress SourceAddress { get; }
        public IMessageBusAddress DestinationAddress { get; }

        public MessageHeader(IMessageBusAddress sourceAddress, IMessageBusAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }
}