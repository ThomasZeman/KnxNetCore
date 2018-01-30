namespace KnxNetCore.MessageBus
{
    public interface IMessageBusInlet
    {
        void Send(IMessageBusAddress destinationAddress, IMessagePayload message);
    }
}