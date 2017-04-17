using System;

namespace KnxNetCore.Telegrams
{
    /// <summary>
    /// Represents a 3-level group address
    /// </summary>
    /// <remarks>2-level addresses or free address structure are not supported</remarks>
    public sealed class GroupAddress
    {
        private static readonly int MainGroupBitWidth = 5;
        private static readonly int MiddleGroupBithWidth = 3;

        private static readonly int MainGroupMax = (1 << MainGroupBitWidth) - 1;
        private static readonly int MiddleGroupMax = (1 << MiddleGroupBithWidth) - 1;

        private static readonly int MiddleGroupBitShift = 8;
        private static readonly int MainGroupBitShift = 11;

        public static GroupAddress FromUShort(ushort groupAddressAsUShort)
        {
            return new GroupAddress(groupAddressAsUShort);
        }

        public static GroupAddress FromGroups(byte mainGroup, byte middleGroup, byte subGroup)
        {
            return new GroupAddress(mainGroup, middleGroup, subGroup);
        }

        private GroupAddress(ushort groupAddressAsUShort)
        {
            AsUShort = groupAddressAsUShort;
        }

        private GroupAddress(byte mainGroup, byte middleGroup, byte subGroup)
        {
            if (mainGroup > MainGroupMax)
            {
                throw new ArgumentOutOfRangeException($"MainGroup cannot be greater than {MainGroupMax} but was {mainGroup}");
            }
            if (middleGroup > MiddleGroupMax)
            {
                throw new ArgumentOutOfRangeException($"MiddleGroup cannot be greater than {MiddleGroupMax} but was {middleGroup}");
            }
            AsUShort = (ushort)(subGroup + (middleGroup << MiddleGroupBitShift) + (mainGroup << MainGroupBitShift));
        }

        public ushort AsUShort { get; }

        public byte[] As3Level => new[]
        {
            (byte) (AsUShort >> MainGroupBitShift),
            (byte) ((AsUShort >> MiddleGroupBitShift) & MiddleGroupMax),
            (byte) AsUShort
        };

        public override string ToString()
        {
            return string.Join("/", As3Level);
        }

        private bool Equals(GroupAddress other)
        {
            return AsUShort == other.AsUShort;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is GroupAddress && Equals((GroupAddress)obj);
        }

        public override int GetHashCode()
        {
            return AsUShort.GetHashCode();
        }
    }
}
