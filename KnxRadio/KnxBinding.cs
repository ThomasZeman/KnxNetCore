using System.Collections.Immutable;
using KnxNetCore;
using KnxNetCore.Telegrams;

namespace KnxRadio
{
    public class KnxBinding : IMessageSink, IMessageSource
    {
        private readonly KnxConnection _knxConnection;
        private readonly MessageBus _messageBus;
        private IMessageBusInlet _busInlet;

        private ImmutableDictionary<IMessageBusAddress, GroupAddress> _mapping = ImmutableDictionary<IMessageBusAddress, GroupAddress>.Empty;
        private ImmutableDictionary<GroupAddress, IMessageBusAddress> _reverseMapping = ImmutableDictionary<GroupAddress, IMessageBusAddress>.Empty;

        public KnxBinding(KnxConnection knxConnection, MessageBus messageBus, IMessageBusAddress sendingAddress)
        {
            _knxConnection = knxConnection;
            _messageBus = messageBus;
            Address = sendingAddress;
            _busInlet = _messageBus.CreateInletFor(this);
            _knxConnection.KnxEventReceived += _knxConnection_KnxEventReceived;
        }

        public void AddSwitch(GroupAddress groupAddress, IMessageBusAddress address)
        {
            _mapping = _mapping.Add(address, groupAddress);
            _reverseMapping = _reverseMapping.Add(groupAddress, address);
            _messageBus.AddMessageSink(address, this, this);
        }

        private void _knxConnection_KnxEventReceived(KnxConnection arg1, CemiFrame arg2)
        {
            IMessageBusAddress entityAddress;
            if (_reverseMapping.TryGetValue(arg2.DestinationAddress, out entityAddress))
            {
                bool onOff = (arg2.Apdu & 1) == 1;
                _busInlet.Send(entityAddress, new SwitchMessage(onOff));
            }
        }

        public void Receive(Message message)
        {
            GroupAddress groupAddress;
            if (_mapping.TryGetValue(message.MessageHeader.DestinationAddress, out groupAddress))
            {
                var switching = message.MessagePayload as SwitchMessage;
                if (switching != null)
                {
                    SendSwitchTelegramToKnxConnection(groupAddress, switching);
                }
            }
        }

        private void SendSwitchTelegramToKnxConnection(GroupAddress groupAddress, SwitchMessage switching)
        {
            var cemiFrame = new CemiFrame(CemiFrame.MessageCodes.DataRequest,
                 CemiFrame.Control1Flags.DoNotRepeat | CemiFrame.Control1Flags.PriorityLow |
                 CemiFrame.Control1Flags.StandardFrame, CemiFrame.Control2Flags.GroupAddress, IndividualAddress.FromAddressLineDevice(1, 1, 60), groupAddress, 1, (ushort)(0x80 | (switching.SwitchState ? 1 : 0)));
            _knxConnection.SendTunnelingRequest(cemiFrame).Wait();
        }

        public IMessageBusAddress Address { get; }
    }
}
