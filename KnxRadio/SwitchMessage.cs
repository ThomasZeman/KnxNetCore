namespace KnxRadio
{
    public class SwitchMessage : IMessagePayload
    {
        public bool SwitchState { get; }

        public SwitchMessage(bool switchState)
        {
            SwitchState = switchState;
        }
    }
}