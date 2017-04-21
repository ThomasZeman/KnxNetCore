using System;
using System.Collections.Generic;
using System.Linq;
using Amplifier.Units;
using KnxNetCore.Datapoints;
using Xunit;

namespace KnxNetCore.UnitTests.Datapoints
{
    public class Dpt9001Tests
    {
        // Input/Output data has been verified with ETS5
        public static IEnumerable<object[]> TestData => new[]
        {
            new object[] {new byte[] {0x0c, 0x79},new Measure<Celsius>(22.9d)},
            new object[] {new byte[] {0x0c, 0x42}, new Measure<Celsius>(21.8d)},
            new object[] {new byte[] {0x0c, 0x15}, new Measure<Celsius>(20.9d)},
            new object[] {new byte[] {0x47, 0xa1}, new Measure<Celsius>(4999.68d)},
            new object[] {new byte[] {0x64, 0xc4}, new Measure<Celsius>(49971.2d)},
            new object[] {new byte[] {0x64, 0xc4}, new Measure<Celsius>(49971.2d)},
            new object[] {new byte[] {0x8a, 0xc5}, new Measure<Celsius>(-26.78d)},
            new object[] {new byte[] {0xa1, 0x56}, new Measure<Celsius>(-272.96d)},
            new object[] {new byte[] {0x00, 0x00}, new Measure<Celsius>(0d)},
            new object[] {new byte[] {0x00, 0x01}, new Measure<Celsius>(0.01d)},
            new object[] {new byte[] {0x87, 0xff}, new Measure<Celsius>(-0.01d)},
            new object[] {new byte[] {0x00, 0x64}, new Measure<Celsius>(1d)},
            new object[] {new byte[] {0x87, 0x9c}, new Measure<Celsius>(-1d)}
        };

        [Theory, MemberData(nameof(TestData))]
        public void BytesToCelsius(byte[] input, Measure<Celsius> expected)
        {
            var final = new byte[10];
            Array.Copy(input, 0, final, 3, input.Length);
            var result = Dpt9001.BytesToCelsius(new ArraySegment<byte>(final, 3, 2));
            Assert.Equal(expected, result);
        }

        [Theory, MemberData(nameof(TestData))]
        public void CelsiusToBytes(byte[] input, Measure<Celsius> expected)
        {
            byte[] outBytes = new byte[5];
            Dpt9001.CelsiusToBytes(expected, outBytes, 3);
            Assert.Equal(input, outBytes.Skip(3).ToArray());
        }

        [Fact]
        public void Degree_celsius_below_273_throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Dpt9001.BytesToCelsius(new ArraySegment<byte>(new byte[] { 0xf8, 0x02 }, 0, 2)));
        }

        [Fact]
        public void Degree_celsius_above_670760_throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Dpt9001.BytesToCelsius(new ArraySegment<byte>(new byte[] { 0x7f, 0xff }, 0, 2)));
        }
    }
}
