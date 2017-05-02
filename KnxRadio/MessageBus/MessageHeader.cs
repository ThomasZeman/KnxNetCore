namespace KnxRadio
{
    public class MessageHeader
    {
        public IMessageBusAddress SourceAddress { get; private set; }
        public IMessageBusAddress DestinationAddress { get; private set; }

        public MessageHeader(IMessageBusAddress sourceAddress, IMessageBusAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }
}