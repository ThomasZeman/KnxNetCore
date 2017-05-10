namespace KnxRadio
{
    public interface IMessageBusInlet
    {
        void Send(IMessageBusAddress destinationAddress, IMessagePayload message);
    }
}