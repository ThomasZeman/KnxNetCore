using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KnxRadio.Messages;

namespace KnxRadio.Model.Components
{
    public class TemperatureGauge : IComponent
    {
        public void AddedToEntity(Entity entity)
        {
        }

        public Task Receive(Message message)
        {
            var temperatureMessage = message.MessagePayload as TemperatureMessage;
            if (temperatureMessage != null)
            {
                Console.WriteLine("TemperatureGauge: {0}", temperatureMessage.Temperature);
            }
            return Task.FromResult(true);
        }
    }
}
