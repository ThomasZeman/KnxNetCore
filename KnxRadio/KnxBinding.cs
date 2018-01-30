using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Amplifier.Units;
using KnxNetCore;
using KnxNetCore.Knx;
using KnxNetCore.Knx.Datapoints;
using KnxNetCore.Knx.Telegrams;
using KnxNetCore.MessageBus;
using KnxNetCore.Messages;

namespace KnxRadio
{
    public enum KnxAddressBindingTypes
    {
        Switch,
        Temperature
    }

    public class KnxAddressBinding : IEquatable<KnxAddressBinding>
    {
        public GroupAddress GroupAddress { get; }
        public KnxAddressBindingTypes KnxAddressBindingType { get; }

        public KnxAddressBinding(GroupAddress groupAddress, KnxAddressBindingTypes knxAddressBindingType)
        {
            GroupAddress = groupAddress;
            KnxAddressBindingType = knxAddressBindingType;
        }

        public bool Equals(KnxAddressBinding other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(GroupAddress, other.GroupAddress);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((KnxAddressBinding)obj);
        }

        public override int GetHashCode()
        {
            return (GroupAddress != null ? GroupAddress.GetHashCode() : 0);
        }

        public static bool operator ==(KnxAddressBinding left, KnxAddressBinding right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(KnxAddressBinding left, KnxAddressBinding right)
        {
            return !Equals(left, right);
        }
    }

    public class KnxBinding : IMessageSink, IMessageSource
    {
        private readonly KnxConnection _knxConnection;
        private readonly MessageBus _messageBus;
        private IMessageBusInlet _busInlet;

        private NtoMDictionary<BusAddress, GroupAddress> _mapping = new NtoMDictionary<BusAddress, GroupAddress>();
        private ImmutableDictionary<GroupAddress, KnxAddressBindingTypes> _bindingTypes = ImmutableDictionary<GroupAddress, KnxAddressBindingTypes>.Empty;

        public KnxBinding(KnxConnection knxConnection, MessageBus messageBus, BusAddress sendingAddress)
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
            ImmutableList<BusAddress> entityAddresses;
            if (_mapping.Mapping2.TryGetValue(arg2.DestinationAddress, out entityAddresses))
            {

                KnxAddressBindingTypes bindingType;
                if (_bindingTypes.TryGetValue(arg2.DestinationAddress, out bindingType))
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
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(bindingType), bindingType, null);
            }
        }

        public void Receive(Message message)
        {
            ImmutableList<GroupAddress> groupAddresses;
            if (_mapping.Mapping1.TryGetValue(message.MessageHeader.DestinationAddress, out groupAddresses))
            {
                var switching = message.MessagePayload as SwitchMessage;
                if (switching != null)
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

    public class NtoMDictionary<T1, T2>
    {
        private ImmutableDictionary<T1, ImmutableList<T2>> _mapping1 = ImmutableDictionary<T1, ImmutableList<T2>>.Empty;
        private ImmutableDictionary<T2, ImmutableList<T1>> _mapping2 = ImmutableDictionary<T2, ImmutableList<T1>>.Empty;

        public ImmutableDictionary<T1, ImmutableList<T2>> Mapping1 => _mapping1;

        public ImmutableDictionary<T2, ImmutableList<T1>> Mapping2 => _mapping2;

        public void Add(T1 items1, T2 items2)
        {
            _mapping1 = _mapping1.AddToListOfValues(items1, items2);
            _mapping2 = _mapping2.AddToListOfValues(items2, items1);
        }

        public void Add(ICollection<T1> items1, ICollection<T2> items2)
        {
            _mapping1 = _mapping1.AddNtoM(items1, items2);
            _mapping2 = _mapping2.AddNtoM(items2, items1);
        }
    }

    internal static class NtoMExtensions
    {
        public static ImmutableDictionary<T1, ImmutableList<T2>> AddNtoM<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> mapping1, ICollection<T1> items1, ICollection<T2> items2)
        {
            return mapping1.SetItems(
                items1.SelectMany(
                    item1 =>
                    {
                        return items2.Select(item2 => new KeyValuePair<T1, ImmutableList<T2>>(item1, mapping1.GetOrEmpty(item1).Add(item2)));
                    }));
        }
    }

    internal static class ImmutableDictionaryExtensions
    {
        public static ImmutableList<T2> GetOrEmpty<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> dictionary, T1 key)
        {
            ImmutableList<T2> list;
            return dictionary.TryGetValue(key, out list) ? list : ImmutableList<T2>.Empty;
        }

        public static ImmutableDictionary<T1, ImmutableList<T2>> AddToListOfValues<T1, T2>(this ImmutableDictionary<T1, ImmutableList<T2>> dictionary, T1 key, T2 item)
        {
            return dictionary.SetItem(key, dictionary.GetOrEmpty(key).Add(item));
        }
    }
}
