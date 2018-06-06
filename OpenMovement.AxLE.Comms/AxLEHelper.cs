using System;

namespace OpenMovement.AxLE.Comms
{
    public class AxLEHelper
    {
        public static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ShortToHexWordsLE(UInt16 value)
        {
            var little = value & 0xFFul;
            var result = ((little << 8) | ((ulong)(value >> 8) & 0xFFul));

            return result.ToString("X");
        }
    }
}