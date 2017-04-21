using System;
using System.Collections.Generic;
using System.Linq;
using KnxNetCore.Datapoints;
using Xunit;

namespace KnxNetCore.UnitTests.Datapoints
{
    public class Dpt9Tests
    {
        // Input/Output data has been verified with ETS5
        public static IEnumerable<object[]> TestData => new[]
        {
            new object[] {new byte[] {0x0c, 0x79}, 22.9d},
            new object[] {new byte[] {0x0c, 0x42}, 21.8d},
            new object[] {new byte[] {0x0c, 0x15}, 20.9d},
            new object[] {new byte[] {0x47, 0xa1}, 4999.68d},
            new object[] {new byte[] {0x64, 0xc4}, 49971.2d},
            new object[] {new byte[] {0x64, 0xc4}, 49971.2d},
            new object[] {new byte[] {0x7f, 0xff}, 670760.96d},
            new object[] {new byte[] {0xf8, 0x02}, -670433.28d},
            new object[] {new byte[] {0xf8, 0x01}, -670760.96d},
            new object[] {new byte[] {0x8a, 0xc5}, -26.78d},
            new object[] {new byte[] {0xa1, 0x56}, -272.96d},
            new object[] {new byte[] {0x00, 0x00}, 0d},
            new object[] {new byte[] {0x00, 0x01}, 0.01d},
            new object[] {new byte[] {0x87, 0xff}, -0.01d},
            new object[] {new byte[] {0x00, 0x64}, 1d},
            new object[] {new byte[] {0x87, 0x9c}, -1d}
        };

        [Theory, MemberData(nameof(TestData))]
        public void BytesToDouble(byte[] input, double expected)
        {
            var final = new byte[10];
            Array.Copy(input, 0, final, 3, input.Length);
            var result = Dpt9.BytesToDouble(new ArraySegment<byte>(final, 3, 2));
            Assert.Equal(expected, result, 3);
        }

        [Theory, MemberData(nameof(TestData))]
        public void DoubleToBytes(byte[] input, double expected)
        {
            byte[] outBytes = new byte[5];
            Dpt9.DoubleToBytes(expected, outBytes, 3);
            Assert.Equal(input, outBytes.Skip(3).ToArray());
        }

        [Fact]
        public void Throws_when_not_enough_space_in_array()
        {
            byte[] outBytes = new byte[4];
            Assert.Throws<ArgumentException>(() => Dpt9.DoubleToBytes(0d, outBytes, 3));
        }

        [Fact]
        public void Throws_when_not_enough_space_in_array2()
        {
            byte[] outBytes = new byte[1];
            Assert.Throws<ArgumentException>(() => Dpt9.DoubleToBytes(0d, outBytes, 0));
        }

        [Fact]
        public void Throws_when_input_float_is_too_big()
        {
            byte[] outBytes = new byte[10];
            Assert.Throws<ArgumentOutOfRangeException>(() => Dpt9.DoubleToBytes(double.MaxValue, outBytes, 0));
        }

        [Fact]
        public void Throws_when_input_float_is_too_big2()
        {
            byte[] outBytes = new byte[10];
            Assert.Throws<ArgumentOutOfRangeException>(() => Dpt9.DoubleToBytes(1000000000d, outBytes, 0));
        }

        [Fact]
        public void Not_enough_data_throws()
        {
            Assert.Throws<ArgumentException>(() => Dpt9.BytesToDouble(new ArraySegment<byte>(new byte[] { 0x7f, 0xff }, 1, 1)));
        }

        [Fact]
        public void Too_much_data_throws()
        {
            Assert.Throws<ArgumentException>(() => Dpt9.BytesToDouble(new ArraySegment<byte>(new byte[] { 0x7f, 0xff, 0x44, 0x33 }, 1, 3)));
        }
    }
}
