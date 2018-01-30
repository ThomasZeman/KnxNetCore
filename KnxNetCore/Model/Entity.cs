using System.Collections.Generic;
using System.Linq;
using KnxNetCore.MessageBus;

namespace KnxNetCore.Model
{
    public class Entity : IMessageSink, IMessageSource
    {
        public IMessageBusAddress Address { get; private set; }

        private readonly IComponent[] _components;
        public IMessageBusInlet Inlet { get; }

        public Entity(MessageBus.MessageBus messageBus, IMessageBusAddress address, IEnumerable<IComponent> components)
        {
            Inlet = messageBus.CreateInletFor(this);
            Address = address;
            _components = components.ToArray();
            for (int i = 0; i < _components.Length; i++)
            {
                _components[i].AddedToEntity(this);
            }
            messageBus.AddMessageSink(address, this, this);
        }

        public void Receive(Message message)
        {
            for (int i = 0; i < _components.Length; i++)
            {
                _components[i].Receive(message);
            }
        }
    }
}
