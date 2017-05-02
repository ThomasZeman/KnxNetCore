using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;

namespace KnxRadio
{
    public class Entity : IMessageSink, IMessageSource
    {
        public IEntityAddress Address { get; private set; }

        private IComponent[] _components;
        public IMessageBusInlet Inlet { get; }

        public Entity(MessageBus messageBus, IEntityAddress address, IEnumerable<IComponent> components)
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

        //public async Task Receive(Message message)
        //{
        //    if (!Equals(message.DestinationAddress, Address))
        //    {
        //        return;
        //    }
        //    for (int i = 0; i < _components.Length; i++)
        //    {
        //        await _components[i].Receive(message);
        //    }
        //}

        public void Receive(Message message)
        {
            for (int i = 0; i < _components.Length; i++)
            {
                _components[i].Receive(message);
            }
        }
    }
}
