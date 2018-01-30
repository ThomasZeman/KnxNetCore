using System;
using System.Threading.Tasks;
using KnxNetCore.MessageBus;
using KnxNetCore.Messages;

namespace KnxNetCore.Model.Components
{
    public class Switch : IComponent
    {
        private Entity _entity;
        public bool State { get; private set; }

        public void AddedToEntity(Entity entity)
        {
            _entity = entity;
        }

        public async Task Receive(Message message)
        {
            bool? switchMessageState = (message.MessagePayload as SwitchMessage)?.SwitchState;
            if (switchMessageState.HasValue)
            {
                State = switchMessageState.Value;
                Console.WriteLine($"Address {_entity.Address} Changed to: {State}");
            }
        }
    }
}