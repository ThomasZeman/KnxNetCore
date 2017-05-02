using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

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

    public interface IComponent
    {
        void AddedToEntity(Entity entity);

        Task Receive(Message message);
    }

    public class Message
    {
        public MessageHeader MessageHeader { get; }
        public IMessagePayload MessagePayload { get; }

        public Message(MessageHeader messageHeader, IMessagePayload messagePayload)
        {
            MessageHeader = messageHeader;
            MessagePayload = messagePayload;
        }
    }

    public class MessageHeader
    {
        public IEntityAddress SourceAddress { get; private set; }
        public IEntityAddress DestinationAddress { get; private set; }

        public MessageHeader(IEntityAddress sourceAddress, IEntityAddress destinationAddress)
        {
            SourceAddress = sourceAddress;
            DestinationAddress = destinationAddress;
        }
    }

    public interface IMessagePayload
    {

    }

    public class SwitchMessage : IMessagePayload
    {
        public bool SwitchState { get; }

        public SwitchMessage(bool switchState)
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
            bool? switchMessageState = (message.MessagePayload as SwitchMessage)?.SwitchState;
            if (switchMessageState.HasValue)
            {
                State = switchMessageState.Value;
                Console.WriteLine($"Address {_entity.Address} Changed to: {State}");
            }
        }
    }

    public class Button : IComponent
    {
        private Entity _entity;
        private readonly IEntityAddress _targetEntityAddress;
        private bool _switchState;

        public Button(IEntityAddress targetEntityAddress)
        {
            _targetEntityAddress = targetEntityAddress;
        }

        public void AddedToEntity(Entity entity)
        {
            _entity = entity;
        }

        public async Task Receive(Message message)
        {
        }

        public void Switch()
        {
            _switchState = !_switchState;
            _entity.Inlet.Send(_targetEntityAddress, new SwitchMessage(_switchState));
        }
    }

    public class MessageBus
    {
        // Design: Keep address format flexible atm. Instead of genericizing everything with some TAddress do dynamic
        // checks and casts to reduce noise in type definitions

        // Design: What to guarantees to achieve? 
        // 1. All Receivers done before Bus processes next message? No diff. between async and sync except for thread being free'd
        // 2. Requirement that all relevant receivers get called at the same time? Only partly achievable with async sig because bad implementation could still block sync
        // 3. NFC Scalability: How to allow broadcast to thousands of receivers
        // 4. NFC Performance High Throughput: How to ensure high processing rate of messages
        // 5. NFC Fault Tolerance: What to do when components throw exceptions

        // Design: async signatures really necessary?

        // Dependency during creation: Bus and Entities need to reference each other in some way so "immutability first" cannot apply for one of the them. 
        // Choosing: Bus is there first, entities come (and go?) later

        class SinkData
        {
            public IMessageSink Sink { get; }
            public int CorrelatingSourceId { get; }

            public SinkData(IMessageSink sink, int correlatingSourceId)
            {
                Sink = sink;
                CorrelatingSourceId = correlatingSourceId;
            }
        }

        ImmutableDictionary<IEntityAddress, ImmutableList<SinkData>> _sinkMapping = ImmutableDictionary<IEntityAddress, ImmutableList<SinkData>>.Empty;
        ImmutableDictionary<IMessageSource, int> _sourceToCorrelationId = ImmutableDictionary<IMessageSource, int>.Empty;
        private int _sourceIdCounter = 0;

        private void Send(Message message, int correlatingSourceId)
        {
            ImmutableList<SinkData> sinkList;
            if (_sinkMapping.TryGetValue(message.MessageHeader.DestinationAddress, out sinkList))
            {
                foreach (var messageSink in sinkList)
                {
                    if (messageSink.CorrelatingSourceId != correlatingSourceId)
                    {
                        messageSink.Sink.Receive(message);
                    }
                }
            }
        }

        public IMessageBusInlet CreateInletFor(IMessageSource messageSource)
        {
            var newSourceIdCounter = _sourceIdCounter++;
            _sourceToCorrelationId = _sourceToCorrelationId.Add(messageSource, newSourceIdCounter);
            return new MessageBusInlet(this, messageSource, newSourceIdCounter);
        }

        internal void AddMessageSink(IEntityAddress listeningFor, IMessageSink sink, IMessageSource correlatingSource)
        {
            ImmutableList<SinkData> list;
            int correlatingSourceId;
            if (!_sourceToCorrelationId.TryGetValue(correlatingSource, out correlatingSourceId))
            {
                correlatingSourceId = -1;
            }
            _sinkMapping = _sinkMapping.SetItem(listeningFor, _sinkMapping.TryGetValue(listeningFor, out list) ?
                list.Add(new SinkData(sink, correlatingSourceId)) :
                ImmutableList<SinkData>.Empty.Add(new SinkData(sink, correlatingSourceId)));

        }

        private class MessageBusInlet : IMessageBusInlet
        {
            private readonly MessageBus _messageBus;
            private readonly IMessageSource _messageSource;
            private readonly int _correlatingSourceId;

            public MessageBusInlet(MessageBus messageBus, IMessageSource messageSource, int correlatingSourceId)
            {
                _messageBus = messageBus;
                _messageSource = messageSource;
                _correlatingSourceId = correlatingSourceId;
            }

            public void Send(IEntityAddress destinationAddress, IMessagePayload message)
            {
                var sourceAddress = _messageSource.Address;
                _messageBus.Send(new Message(new MessageHeader(sourceAddress, destinationAddress), message), _correlatingSourceId);
            }
        }
    }

    public interface IMessageSink
    {

        void Receive(Message message);
    }

    public interface IMessageSource
    {
        IEntityAddress Address { get; }
    }

    public interface IMessageBusInlet
    {
        void Send(IEntityAddress destinationAddress, IMessagePayload message);
    }
}
