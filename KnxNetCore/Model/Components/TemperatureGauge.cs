using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amplifier.Units;
using KnxNetCore.MessageBus;
using KnxNetCore.Messages;

namespace KnxNetCore.Model.Components
{
    public class TemperatureGauge : IComponent
    {
        private readonly List<Measure<Celsius>> _temperatures = new List<Measure<Celsius>>();

        public List<Measure<Celsius>> Temperatures => _temperatures;

        public void AddedToEntity(Entity entity)
        {
        }

        public Task Receive(Message message)
        {
            var temperatureMessage = message.MessagePayload as TemperatureMessage;
            if (temperatureMessage != null)
            {
                Console.WriteLine("TemperatureGauge: {0}", temperatureMessage.Temperature);
                _temperatures.Add(temperatureMessage.Temperature);
            }
            return Task.FromResult(true);
        }
    }
}
