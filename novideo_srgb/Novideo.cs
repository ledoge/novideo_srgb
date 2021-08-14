using System;
using System.Runtime.InteropServices;
using EDIDParser;
using MathNet.Numerics.LinearAlgebra;
using NvAPIWrapper.GPU;

/*
no NDAs violated here!
everything figured out from publicly available information and/or
throwing stuff at NVAPI and observing the results or errors
*/

namespace novideo_srgb
{
    public static class Novideo
    {
        /*
        observed pipeline: content degamma -> content to srgb -> srgb to monitor -> matrix2 -> matrix1 -> monitor gamma
        in hardware all the matrix stuff is done with a single matrix, i.e. these four multiplied together
        3x4 matrices cannot be multiplied with each other though, so only the 3x3 parts are combined "properly"
        and the offsets are simply added together
        */
        [StructLayout(LayoutKind.Sequential)]
        private struct CscV1
        {
            public uint version; // 0x1007C

            public uint
                contentColorSpace; // built-in degamut/degamma transforms, 1 <= x <= 12, default 2 (probably srgb)

            public uint monitorColorSpace; // built-in gamut/gamma transforms, 0 <= x <= 12, default 0 (= csc disabled)
            public uint unknown1; // no idea, set to 0 by both get and set functions -> some type of error code?
            public uint unknown2; // also no idea, not modified by either function -> unused?
            public uint useMatrix1; // 1 to enable
            public unsafe fixed float matrix1[3 * 4]; // r/g/b gain and offset
            public uint useMatrix2;
            public unsafe fixed float matrix2[3 * 4];
        }

        private const uint _NvAPI_GPU_GetColorSpaceConversion = 0x8159E87A;
        private const uint _NvAPI_GPU_SetColorSpaceConversion = 0x0FCABD23A;

        [DllImport("nvapi64", EntryPoint = "nvapi_QueryInterface")]
        private static extern IntPtr NvAPI_QueryInterface(uint id);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvAPI_GPU_GetColorSpaceConversion_t(uint displayId,
            [MarshalAs(UnmanagedType.Struct)] ref CscV1 csc);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int NvAPI_GPU_SetColorSpaceConversion_t(uint displayId,
            [MarshalAs(UnmanagedType.Struct)] ref CscV1 csc);

        private static NvAPI_GPU_GetColorSpaceConversion_t NvAPI_GPU_GetColorSpaceConversion;
        private static NvAPI_GPU_SetColorSpaceConversion_t NvAPI_GPU_SetColorSpaceConversion;

        public struct ColorSpaceConversion
        {
            public uint contentColorSpace;
            public uint monitorColorSpace;
            public float[,] matrix1;
            public float[,] matrix2;
        }

        public static ColorSpaceConversion GetColorSpaceConversion(GPUOutput output)
        {
            var displayId = output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId;

            var csc = new CscV1 {version = 0x1007C};
            var status = NvAPI_GPU_GetColorSpaceConversion(displayId, ref csc);
            if (status != 0)
            {
                throw new Exception("NvAPI_GPU_GetColorSpaceConversion failed with error code " + status);
            }

            var result = new ColorSpaceConversion
            {
                contentColorSpace = csc.contentColorSpace, monitorColorSpace = csc.monitorColorSpace
            };

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    unsafe
                    {
                        if (csc.useMatrix1 == 1)
                        {
                            if (result.matrix1 == null) result.matrix1 = new float[3, 4];
                            result.matrix1[i, j] = csc.matrix1[i * 4 + j];
                        }

                        if (csc.useMatrix2 == 1)
                        {
                            if (result.matrix2 == null) result.matrix2 = new float[3, 4];
                            result.matrix2[i, j] = csc.matrix2[i * 4 + j];
                        }
                    }
                }
            }

            return result;
        }

        public static void SetColorSpaceConversion(GPUOutput output, ColorSpaceConversion conversion)
        {
            var displayId = output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId;

            var csc = new CscV1
            {
                version = 0x1007C,
                contentColorSpace = conversion.contentColorSpace,
                monitorColorSpace = conversion.monitorColorSpace
            };

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    unsafe
                    {
                        if (conversion.matrix1 != null)
                        {
                            csc.useMatrix1 = 1;
                            csc.matrix1[i * 4 + j] = conversion.matrix1[i, j];
                        }

                        if (conversion.matrix2 != null)
                        {
                            csc.useMatrix2 = 1;
                            csc.matrix2[i * 4 + j] = conversion.matrix2[i, j];
                        }
                    }
                }
            }

            var status = NvAPI_GPU_SetColorSpaceConversion(displayId, ref csc);
            if (status != 0)
            {
                throw new Exception("NvAPI_GPU_SetColorSpaceConversion failed with error code " + status);
            }
        }

        public static bool IsColorSpaceConversionActive(GPUOutput output)
        {
            return GetColorSpaceConversion(output).monitorColorSpace != 0;
        }

        public static void DisableColorSpaceConversion(GPUOutput output)
        {
            SetColorSpaceConversion(output, new ColorSpaceConversion {contentColorSpace = 2});
        }

        public static EDID GetEDID(GPUOutput output)
        {
            return new EDID(output.PhysicalGPU.ReadEDIDData(output));
        }

        public static ColorSpaceConversion MatrixToColorSpaceConversion(Matrix<double> matrix)
        {
            var csc = new ColorSpaceConversion
            {
                contentColorSpace = 2, monitorColorSpace = 2, matrix1 = new float[3, 4]
            };

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    csc.matrix1[i, j] = (float) matrix[i, j];
                }
            }

            return csc;
        }

        static Novideo()
        {
            NvAPI_GPU_GetColorSpaceConversion =
                Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_GetColorSpaceConversion_t>(
                    NvAPI_QueryInterface(_NvAPI_GPU_GetColorSpaceConversion));

            NvAPI_GPU_SetColorSpaceConversion =
                Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_SetColorSpaceConversion_t>(
                    NvAPI_QueryInterface(_NvAPI_GPU_SetColorSpaceConversion));
        }
    }
}