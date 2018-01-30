using System;
using System.Collections.Immutable;
using KnxNetCore.Knx.Datapoints;
using KnxNetCore.Knx.Telegrams;
using KnxNetCore.MessageBus;
using KnxNetCore.Messages;

namespace KnxNetCore.Knx
{
    public enum KnxAddressBindingTypes
    {
        Switch,
        Temperature
    }

    public class KnxBinding : IMessageSink, IMessageSource
    {
        private readonly KnxConnection _knxConnection;
        private readonly MessageBus.MessageBus _messageBus;
        private readonly IMessageBusInlet _busInlet;

        private readonly NtoMDictionary<BusAddress, GroupAddress> _mapping = new NtoMDictionary<BusAddress, GroupAddress>();
        private ImmutableDictionary<GroupAddress, KnxAddressBindingTypes> _bindingTypes = ImmutableDictionary<GroupAddress, KnxAddressBindingTypes>.Empty;

        public KnxBinding(KnxConnection knxConnection, MessageBus.MessageBus messageBus, BusAddress sendingAddress)
        {
            _knxConnection = knxConnection;
            _messageBus = messageBus;
            Address = sendingAddress;
            _busInlet = _messageBus.CreateInletFor(this);
            _knxConnection.KnxEventReceived += _knxConnection_KnxEventReceived;
        }

        public void AddSwitch(GroupAddress groupAddress, BusAddress address, KnxAddressBindingTypes knxAddressBindingType)
        {
            _mapping.Add(address, groupAddress);
            _bindingTypes = _bindingTypes.Add(groupAddress, knxAddressBindingType);
            _messageBus.AddMessageSink(address, this, this);
        }

        private void _knxConnection_KnxEventReceived(KnxConnection arg1, CemiFrame arg2)
        {
            if (_mapping.Mapping2.TryGetValue(arg2.DestinationAddress, out var entityAddresses))
            {
                if (_bindingTypes.TryGetValue(arg2.DestinationAddress, out var bindingType))
                {
                    var message = CreateBusMessage(bindingType, arg2);
                    foreach (var messageBusAddress in entityAddresses)
                    {
                        _busInlet.Send(messageBusAddress, message);
                    }
                }
            }
        }

        private IMessagePayload CreateBusMessage(KnxAddressBindingTypes bindingType, CemiFrame cemiFrame)
        {
            switch (bindingType)
            {
                case KnxAddressBindingTypes.Switch:
                    bool onOff = (cemiFrame.Apdu & 1) == 1;
                    return new SwitchMessage(onOff);
                case KnxAddressBindingTypes.Temperature:
                    return new TemperatureMessage(Dpt9001.BytesToCelsius(new ArraySegment<byte>(cemiFrame.Data.Array, cemiFrame.Data.Offset, cemiFrame.Data.Count)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingType), bindingType, null);
            }
        }

        public void Receive(Message message)
        {
            if (_mapping.Mapping1.TryGetValue(message.MessageHeader.DestinationAddress, out var groupAddresses))
            {
                if (message.MessagePayload is SwitchMessage switching)
                {
                    foreach (var groupAddress in groupAddresses)
                    {
                        SendSwitchTelegramToKnxConnection(groupAddress, switching);
                    }
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


        public BusAddress Address { get; }
    }
}
