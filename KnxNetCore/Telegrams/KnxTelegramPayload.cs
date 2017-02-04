namespace KnxNetCore.Telegrams
{
    internal abstract class KnxTelegramPayload
    {
        public abstract void Accept(IKnxTelegramVisitor knxTelegramVisitor);
    }
}