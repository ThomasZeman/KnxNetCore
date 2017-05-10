using System;
using System.Collections.Generic;
using System.Text;
using Amplifier.Units;
using KnxNetCore;

namespace KnxRadio.Messages
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
