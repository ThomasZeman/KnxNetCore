using System;

namespace KnxRadio
{
    class IntegerEntityAddress : IEntityAddress, IEquatable<IntegerEntityAddress>
    {
        public int Value { get; }

        public IntegerEntityAddress(int value)
        {
            Value = value;
        }

        public bool Equals(IntegerEntityAddress other)
        {
            if (Object.ReferenceEquals(null, other)) return false;
            if (Object.ReferenceEquals(this, other)) return true;
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(null, obj)) return false;
            if (Object.ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IntegerEntityAddress)obj);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(IntegerEntityAddress left, IntegerEntityAddress right)
        {
            return Object.Equals(left, right);
        }

        public static bool operator !=(IntegerEntityAddress left, IntegerEntityAddress right)
        {
            return !Object.Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}