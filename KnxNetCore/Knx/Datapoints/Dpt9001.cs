using System;
using Amplifier.Units;

namespace KnxNetCore.Knx.Datapoints
{
    public static class Dpt9001
    {
        public static Measure<Celsius> BytesToCelsius(ArraySegment<byte> input)
        {
            var result = Dpt9.BytesToDouble(input);
            if (result < -273 || result > 670760)
            {
                throw new ArgumentOutOfRangeException(nameof(input),$"Input results in {result} degree Celsius which is out of the specified range");
            }
            return new Measure<Celsius>(result);
        }

        public static void CelsiusToBytes(Measure<Celsius> value, byte[] bytes, int offset)
        {
            Dpt9.DoubleToBytes((double)value.Value, bytes, offset);
        }
    }
}