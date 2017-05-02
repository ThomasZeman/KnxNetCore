using System.Collections.Immutable;

namespace KnxRadio
{
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

        ImmutableDictionary<IMessageBusAddress, ImmutableList<SinkData>> _sinkMapping = ImmutableDictionary<IMessageBusAddress, ImmutableList<SinkData>>.Empty;
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

        internal void AddMessageSink(IMessageBusAddress listeningFor, IMessageSink sink, IMessageSource correlatingSource)
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

            public void Send(IMessageBusAddress destinationAddress, IMessagePayload message)
            {
                var sourceAddress = _messageSource.Address;
                _messageBus.Send(new Message(new MessageHeader(sourceAddress, destinationAddress), message), _correlatingSourceId);
            }
        }
    }
}