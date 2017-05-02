namespace KnxRadio
{
    public class MessageHeader
    {
        public IEntityAddress SourceAddress { get; private set; }
        public IEntityAddress DestinationAddress { get; private set; }

        public MessageHeader(IEntityAddress sourceAddress, IEntityAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }
}