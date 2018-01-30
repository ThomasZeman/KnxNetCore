namespace KnxNetCore.MessageBus
{
    public interface IMessageSink
    {

        void Receive(Message message);
    }
}