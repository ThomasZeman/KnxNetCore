namespace KnxRadio
{
    public interface IMessageSink
    {

        void Receive(Message message);
    }
}