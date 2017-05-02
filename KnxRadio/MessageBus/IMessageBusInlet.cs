namespace KnxRadio
{
    public interface IMessageBusInlet
    {
        void Send(IEntityAddress destinationAddress, IMessagePayload message);
    }
}