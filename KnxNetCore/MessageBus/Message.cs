namespace KnxNetCore.MessageBus
{
    public class Message
    {
        public MessageHeader MessageHeader { get; }
        public IMessagePayload MessagePayload { get; }

        public Message(MessageHeader messageHeader, IMessagePayload messagePayload)
        {
            MessageHeader = messageHeader;
            MessagePayload = messagePayload;
        }
    }
}