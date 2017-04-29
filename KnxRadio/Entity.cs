using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KnxRadio
{
    public class Entity
    {
        public IEntityAddress Address { get; private set; }

        private IComponent[] _components;

        public Entity(IEntityAddress address, IEnumerable<IComponent> components)
        {
            Address = address;
            _components = components.ToArray();
            for (int i = 0; i < _components.Length; i++)
            {
                _components[i].AddedToEntity(this);
            }
        }

        public async Task Receive(Message message)
        {
            if (!Equals(message.DestinationAddress, Address))
            {
                return;
            }
            for (int i = 0; i < _components.Length; i++)
            {
                await _components[i].Receive(message);
            }
        }
    }

    public interface IComponent
    {
        void AddedToEntity(Entity entity);

        Task Receive(Message message);
    }

    public class Message
    {
        public IEntityAddress SourceAddress { get; private set; }
        public IEntityAddress DestinationAddress { get; private set; }

        public Message(IEntityAddress sourceAddress, IEntityAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }

    public class SwitchMessage : Message
    {
        public bool SwitchState { get; }

        public SwitchMessage(IEntityAddress sourceAddress, IEntityAddress destinationAddress, bool switchState) : base(sourceAddress, destinationAddress)
        {
            SwitchState = switchState;
        }
    }

    public interface IEntityAddress
    {

    }

    class IntegerEntityAddress : IEntityAddress, IEquatable<IntegerEntityAddress>
    {
        public int Value { get; }

        public IntegerEntityAddress(int value)
        {
            Value = value;
        }

        public bool Equals(IntegerEntityAddress other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntegerEntityAddress)obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(IntegerEntityAddress left, IntegerEntityAddress right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IntegerEntityAddress left, IntegerEntityAddress right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }

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
            bool? switchMessageState = (message as SwitchMessage)?.SwitchState;
            if (switchMessageState.HasValue)
            {
                State = switchMessageState.Value;
                Console.WriteLine($"Address {_entity.Address} Changed to: {State}");
            }
        }
    }

    public class Button : IComponent
    {
        private readonly IEntityAddress _targetEntityAddress;

        public Button(IEntityAddress targetEntityAddress)
        {
            _targetEntityAddress = targetEntityAddress;
        }

        public void AddedToEntity(Entity entity)
        {
        }

        public async Task Receive(Message message)
        {
        }

        public void Switch(bool switchState)
        {

        }
    }

    public class MessageBus
    {
        // Design: Keep address format flexible atm. Instead of genericizing everything with some TAddress do dynamic
        // checks and casts to reduce noise in type definitions

        Dictionary<IEntityAddress, Entity> _entities = new Dictionary<IEntityAddress, Entity>();

        public MessageBus(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                _entities.Add(entity.Address, entity);
            }
        }

        // Design: async signatures really necessary?

        public void Send(IEntityAddress destinationAddress, Message message)
        {
            Entity entity;
            if (_entities.TryGetValue(destinationAddress, out entity))
            {
                entity.Receive(message);
            }
        }
    }
}
