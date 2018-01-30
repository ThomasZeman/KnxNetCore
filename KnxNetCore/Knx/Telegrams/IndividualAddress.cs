using System;

namespace KnxNetCore.Knx.Telegrams
{
    /// <summary>
    ///     Represents a 3-level individual address
    /// </summary>
    public sealed class IndividualAddress
    {
        //TODO: Consider putting communalities of IndividualAddress and GroupAddress into base class to reduce basically duplicate code

        private static readonly int AreaBitWidth = 4;
        private static readonly int LineBitWidth = 4;

        private static readonly int AreaMax = (1 << AreaBitWidth) - 1;
        private static readonly int LineMax = (1 << LineBitWidth) - 1;

        private static readonly int AreaBitShift = 12;
        private static readonly int LineBitShift = 8;

        private IndividualAddress(ushort asUShort)
        {
            AsUShort = asUShort;
        }

        private IndividualAddress(byte area, byte line, byte device)
        {
            if (area > AreaMax)
            {
                throw new ArgumentOutOfRangeException($"Area cannot be greater than {AreaMax} but was {area}");
            }

            if (line > LineMax)
            {
                throw new ArgumentOutOfRangeException($"Line cannot be greater than {LineMax} but was {line}");
            }

            AsUShort = (ushort) (device | (line << LineBitShift) | (area << AreaBitShift));
        }

        public ushort AsUShort { get; }

        public byte[] As3Level => new[]
        {
            (byte) (AsUShort >> AreaBitShift),
            (byte) ((AsUShort >> LineBitShift) & LineMax),
            (byte) AsUShort
        };

        public static IndividualAddress FromUShort(ushort asUShort)
        {
            return new IndividualAddress(asUShort);
        }

        public static IndividualAddress FromAddressLineDevice(byte area, byte line, byte device)
        {
            return new IndividualAddress(area, line, device);
        }

        public override string ToString()
        {
            return string.Join("/", As3Level);
        }

        private bool Equals(IndividualAddress other)
        {
            return AsUShort == other.AsUShort;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is IndividualAddress && Equals((IndividualAddress) obj);
        }

        public override int GetHashCode()
        {
            return AsUShort.GetHashCode();
        }
    }
}