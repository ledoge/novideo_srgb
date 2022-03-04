using novideo_srgb.core.Models;
using System.Runtime.InteropServices;

namespace novideo_srgb.core.Nvidia;

internal static partial class Novideo
{
    [StructLayout(LayoutKind.Sequential)]
    private struct Csc
    {
        public uint version; // 0x1007C for V1, 0x200A0 for V2

        public uint contentColorSpace; // built-in degamut/degamma transforms, 1 <= x <= 12, default 2 (probably srgb)

        public uint monitorColorSpace; // built-in gamut/gamma transforms, 0 <= x <= 12, default 0 (= csc disabled)
        public uint unknown1; // no idea, set to 0 by both get and set functions -> some type of error code?
        public uint unknown2; // also no idea, not modified by either function -> unused?
        public uint useMatrix1; // 1 to enable
        public unsafe fixed float matrix1[3 * 4]; // r/g/b gain and offset
        public uint useMatrix2;

        public unsafe fixed float matrix2[3 * 4];

        // v2 stuff
        public unsafe float* degamma; // pointer to degamma part of buffer (= first element)
        public unsafe float* regamma; // pointer to regamma part of buffer (= index 0x3000)

        public unsafe float* buffer; // float array of size 0x6000, contains interleaved rgb degamma followed by regamma

        public int bufferSize; // 0x6000
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Dither
    {
        public uint version;
        public DitherControl ditherControl;
    }

    private struct ColorSpaceConversion
    {
        public uint contentColorSpace;
        public uint monitorColorSpace;
        public float[,] matrix1;
        public float[,] matrix2;
    }
}
