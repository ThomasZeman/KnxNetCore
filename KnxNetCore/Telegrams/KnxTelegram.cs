namespace KnxNetCore.Telegrams
{
    internal class KnxTelegram
    {
        public KnxTelegram(KnxTelegramHeader knxTelegramHeader, KnxTelegramPayload knxTelegramPayload)
        {
            KnxTelegramHeader = knxTelegramHeader;
            KnxTelegramPayload = knxTelegramPayload;
        }

        public KnxTelegramHeader KnxTelegramHeader { get; private set; }
        public KnxTelegramPayload KnxTelegramPayload { get; private set; }
    }
}