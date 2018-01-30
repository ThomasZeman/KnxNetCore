using System;

namespace KnxNetCore.MessageBus
{
    public struct BusAddress : IEquatable<BusAddress>
    {
        public bool Equals(BusAddress other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BusAddress && Equals((BusAddress) obj);
        }

        private int Value { get; }

        public BusAddress(int value)
        {
            Value = value;
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(BusAddress left, BusAddress right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(BusAddress left, BusAddress right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}