using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using KnxNetCore;
using KnxNetCore.Telegrams;

namespace KnxRadio
{
    public class KnxBinding : IMessageSink, IMessageSource
    {
        private readonly KnxConnection _knxConnection;
        private readonly MessageBus _messageBus;
        private readonly IEntityAddress _sendingAddress;
        private IMessageBusInlet _busInlet;

        private ImmutableDictionary<IEntityAddress, GroupAddress> _mapping = ImmutableDictionary<IEntityAddress, GroupAddress>.Empty;

        public KnxBinding(KnxConnection knxConnection, MessageBus messageBus, IEntityAddress sendingAddress)
        {
            _knxConnection = knxConnection;
            _messageBus = messageBus;
            _sendingAddress = sendingAddress;
            _busInlet = _messageBus.CreateInletFor(this);
            // Introduce some sort of promiscuous mode for bus or allow sinks to register for several addresses?
            // _messageBus.AddMessageSink(this);
            _knxConnection.KnxEventReceived += _knxConnection_KnxEventReceived;
        }

        public void AddSwitch(GroupAddress groupAddress, IEntityAddress address)
        {
            _mapping = _mapping.Add(address, groupAddress);
            _messageBus.AddMessageSink(address, this);
        }

        private void _knxConnection_KnxEventReceived(KnxConnection arg1, KnxNetCore.Telegrams.CemiFrame arg2)
        {

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

        public IEntityAddress Address => _sendingAddress;
    }
}
