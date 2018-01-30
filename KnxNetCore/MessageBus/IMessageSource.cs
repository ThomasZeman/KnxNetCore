namespace KnxNetCore.MessageBus
{
    public interface IMessageSource
    {
        IMessageBusAddress Address { get; }
    }
}