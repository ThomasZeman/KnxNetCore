namespace KnxNetCore.MessageBus
{
    public interface IMessageSource
    {
        BusAddress Address { get; }
    }
}