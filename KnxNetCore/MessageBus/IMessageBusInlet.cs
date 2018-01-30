namespace KnxNetCore.MessageBus
{
    public interface IMessageBusInlet
    {
        void Send(BusAddress destinationAddress, IMessagePayload message);
    }
}