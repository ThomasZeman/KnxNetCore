using System;

namespace KnxNetCore.Knx.Datapoints
{
    public static class Dpt9
    {
        // based on: http://www.knx.org/fileadmin/template/documents/downloads_support_menu/KNX_tutor_seminar_page/Advanced_documentation/05_Interworking_E1209.pdf
        // chapter 4.10

        public static double BytesToDouble(ArraySegment<byte> arraySegment)
        {
            if (arraySegment.Count != 2)
            {
                throw new ArgumentException($"Must have exactly 2 bytes for decoding a floating point number but had {arraySegment.Count}");
            }
            var array = arraySegment.Array;
            var mantissa = (array[arraySegment.Offset + 1] | (array[arraySegment.Offset] & 7) << 8);
            var exponent = (array[arraySegment.Offset] >> 3) & 15;
            var calculatedExponent = Math.Pow(2, exponent);
            if ((array[arraySegment.Offset] & 128) != 128)
            {
                // positive value
                return Math.Round(0.01d * mantissa * calculatedExponent, 2);
            }
            // Remember: 1 = 0000 0000 0000 0001
            //          -1 = 1111 1111 1111 1111
            //
            // Binary counting is represented in decimal system as [0..32767..-32768..-1]

            // The reverse operation of the description in the knx documentation
            // Substract 1 from mantissa, negate, cut down to 11bit and treat it as negative value
            var mantissaMinusOne = (mantissa - 1);
            var negated = ~mantissaMinusOne;
            var cutTo11Bit = negated & 0x07ff;
            return Math.Round(0.01d * -cutTo11Bit * calculatedExponent, 2);
        }

        public static void DoubleToBytes(double input, byte[] outputBytes, int offset)
        {
            if (outputBytes.Length - offset < 2)
            {
                throw new ArgumentException($"{outputBytes} has length {outputBytes.Length} and does not provide enough space with offset {offset}");
            }
            int exponent = 0;
            var scaled = Math.Round(input * 100d);
            if (double.IsPositiveInfinity(scaled) || double.IsNegativeInfinity(scaled))
            {
                throw new ArgumentOutOfRangeException($"{input} is out of range");
            }
            while (scaled > 2047)
            {
                exponent++;
                scaled /= 2d;
            }
            while (scaled < -2048)
            {
                exponent++;
                scaled /= 2d;
            }
            if (exponent > 15)
            {
                throw new ArgumentOutOfRangeException($"Cannot convert {input} because it would have an exponent of {exponent} which is not supported");
            }
            int mantiassa;
            if (scaled >= 0)
            {
                mantiassa = (int)scaled;
                outputBytes[0] = 0;
            }
            else
            {
                mantiassa = (int)-scaled;
                mantiassa = ~mantiassa;
                mantiassa++;
                outputBytes[offset] = 0x80;
            }
            outputBytes[offset] = (byte)(outputBytes[offset] | (exponent << 3) | (mantiassa >> 8) & 7);
            outputBytes[offset + 1] = (byte)mantiassa;
        }
    }
}
