using KnxNetCore.Telegrams;

namespace KnxNetCore
{
    public class KnxEvent
    {
        public KnxEvent(GroupAddress destinationAddress, bool data)
        {
            DestinationAddress = destinationAddress;
            Data = data;
        }

        public GroupAddress DestinationAddress { get; private set; }
        public bool Data { get; private set; }
    }
}