using System;

namespace KnxNetCore.MessageBus
{
    public class IntegerMessageBusAddress : IMessageBusAddress, IEquatable<IntegerMessageBusAddress>
    {
        private int Value { get; }

        public IntegerMessageBusAddress(int value)
        {
            Value = value;
        }

        public bool Equals(IntegerMessageBusAddress other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((IntegerMessageBusAddress)obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(IntegerMessageBusAddress left, IntegerMessageBusAddress right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IntegerMessageBusAddress left, IntegerMessageBusAddress right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}