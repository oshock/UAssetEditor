using System;
using System.Runtime.InteropServices;

namespace UAssetEditor.IoStore;

// https://github.com/FabianFG/CUE4Parse/blob/b35b843a1193d225df8583b664ef66854557a613/CUE4Parse/Compression/Oodle.cs
public class Oodle
{
    public static unsafe void Decompress(byte[] compressed, int compressedOffset, int compressedSize,
        byte[] uncompressed, int uncompressedOffset, int uncompressedSize)
    {
        long decodedSize;

        fixed (byte* compressedPtr = compressed, uncompressedPtr = uncompressed)
        {
            decodedSize = OodleLZ_Decompress(compressedPtr + compressedOffset, compressedSize,
                uncompressedPtr + uncompressedOffset, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
        }

        if (decodedSize <= 0)
        {
            /*if (reader != null) throw new OodleException(reader, $"Oodle decompression failed with result {decodedSize}");
            throw new OodleException($"Oodle decompression failed with result {decodedSize}");*/
        }

        if (decodedSize < uncompressedSize)
        {
            // Not sure whether this should be an exception or not
        }
    }

    [DllImport("oo2core_9_win64.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern long OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] output, long outputBufferSize, int a, int b, int c, long d, long e, long f, long g, long h, long i, int threadModule);
    [DllImport("oo2core_9_win64.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern unsafe long OodleLZ_Decompress(byte* buffer, long bufferSize, byte* output, long outputBufferSize, int a, int b, int c, long d, long e, long f, long g, long h, long i, int threadModule);

}