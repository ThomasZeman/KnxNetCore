using System;

namespace KnxNetCore.Telegrams
{
    public sealed class IndividualAddress
    {
        public static readonly int AreaBitWidth = 4;
        public static readonly int LineBitWidth = 4;

        public static readonly int AreaMax = (1 << AreaBitWidth) - 1;
        public static readonly int LineMax = (1 << LineBitWidth) - 1;

        private static readonly int AreaBitShift = 12;
        private static readonly int LineBitShift = 8;

        public ushort AsUShort { get; }

        public IndividualAddress(ushort asUShort)
        {
            AsUShort = asUShort;
        }

        public IndividualAddress(byte area, byte line, byte device)
        {
            if (area > AreaMax)
            {
                throw new ArgumentOutOfRangeException($"Area cannot be greater than {AreaMax} but was {area}");
            }
            if (line > LineMax)
            {
                throw new ArgumentOutOfRangeException($"Line cannot be greater than {LineMax} but was {line}");
            }
            AsUShort = (ushort)(device + (line << LineBitShift) + (area << AreaBitShift));
        }

        public byte[] As3Level => new[]
        {
            (byte) (AsUShort >> AreaBitShift),
            (byte) ((AsUShort >> LineBitShift) & LineMax),
            (byte) (AsUShort)
        };
    }
}
