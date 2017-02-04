namespace KnxNetCore
{
    public class KnxEvent
    {
        public KnxEvent(ushort destinationAddress, bool data)
        {
            DestinationAddress = destinationAddress;
            Data = data;
        }

        public ushort DestinationAddress { get; private set; }
        public bool Data { get; private set; }
    }
}