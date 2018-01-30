using Amplifier.Units;
using KnxNetCore.MessageBus;

namespace KnxNetCore.Messages
{
    public class TemperatureMessage : IMessagePayload
    {
        public Measure<Celsius> Temperature { get; }

        public TemperatureMessage(Measure<Celsius> temperature)
        {
            Temperature = temperature;
        }
    }
}
