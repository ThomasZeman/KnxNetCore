namespace KnxNetCore.Knx.Telegrams
{
    internal class KnxTelegram
    {
        public KnxTelegram(KnxTelegramHeader knxTelegramHeader, KnxTelegramPayload knxTelegramPayload)
        {
            KnxTelegramHeader = knxTelegramHeader;
            KnxTelegramPayload = knxTelegramPayload;
        }

        public KnxTelegramHeader KnxTelegramHeader { get; }
        public KnxTelegramPayload KnxTelegramPayload { get; }
    }
}