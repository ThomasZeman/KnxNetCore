using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnxNetCore.Telegrams
{
    /// <summary>
    /// Represents a 3-level group address
    /// </summary>
    /// <remarks>2-level addresses or free address structure are not supported</remarks>
    public class GroupAddress
    {
        public static readonly byte MainGroupMax = 31;
        public static readonly byte MiddleGroupMax = 7;

        public GroupAddress(ushort groupAddressAsUShort)
        {
            AsUShort = groupAddressAsUShort;
        }

        public GroupAddress(byte mainGroup, byte middleGroup, byte subGroup)
        {
            if (mainGroup > MainGroupMax)
            {
                throw new ArgumentOutOfRangeException($"MainGroup cannot be greater than {MainGroupMax} but was {mainGroup}");
            }
            if (middleGroup > MiddleGroupMax)
            {
                throw new ArgumentOutOfRangeException($"MiddleGroup cannot be greater than {MiddleGroupMax} but was {middleGroup}");
            }
            AsUShort = (ushort)(subGroup + (middleGroup << 8) + (mainGroup << 11));
        }

        public ushort AsUShort { get; }

        public byte[] As3Level => new byte[]
        {
            (byte) (AsUShort >> 11),
            (byte) ((AsUShort >> 8) & 7),
            (byte) AsUShort
        };
    }
}
