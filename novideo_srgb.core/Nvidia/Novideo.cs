using EDIDParser;
using Microsoft.Win32;
using novideo_srgb.core.ICCProfile;
using novideo_srgb.core.Models;
using novideo_srgb.core.Models.ToneCurves;
using NvAPIWrapper.Display;
using NvAPIWrapper.GPU;
using System.Runtime.InteropServices;

/*
no NDAs violated here!
everything figured out from publicly available information and/or
throwing stuff at NVAPI and observing the results or errors
*/

namespace novideo_srgb.core.Nvidia;

internal static partial class Novideo
{
    /*
    observed pipeline: content degamma -> content to srgb -> srgb to monitor -> matrix2 -> matrix1 -> monitor gamma
    in hardware all the matrix stuff is done with a single matrix, i.e. these four multiplied together
    3x4 matrices cannot be multiplied with each other though, so only the 3x3 parts are combined "properly"
    and the offsets are simply added together
    */
    
    private const uint _NvAPI_GPU_GetColorSpaceConversion = 0x8159E87A;
    private const uint _NvAPI_GPU_SetColorSpaceConversion = 0x0FCABD23A;
    private const uint _NvAPI_GPU_SetDitherControl = 0x0DF0DFCDD;
    private const uint _NvAPI_GPU_GetDitherControl = 0x932AC8FB;

    [DllImport("nvapi64", EntryPoint = "nvapi_QueryInterface")]
    private static extern IntPtr NvAPI_QueryInterface(uint id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NvAPI_GPU_GetColorSpaceConversion_t(uint displayId,
        [MarshalAs(UnmanagedType.Struct)] ref Csc csc);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NvAPI_GPU_SetColorSpaceConversion_t(uint displayId,
        [MarshalAs(UnmanagedType.Struct)] ref Csc csc);

    private static NvAPI_GPU_GetColorSpaceConversion_t NvAPI_GPU_GetColorSpaceConversion;
    private static NvAPI_GPU_SetColorSpaceConversion_t NvAPI_GPU_SetColorSpaceConversion;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NvAPI_GPU_GetDitherControl_t(uint displayId,
        [MarshalAs(UnmanagedType.Struct)] ref Dither dither);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int NvAPI_GPU_SetDitherControl_t(uint gpuId, uint outputId,
        int state, int bits, int mode);

    private static NvAPI_GPU_GetDitherControl_t NvAPI_GPU_GetDitherControl;
    private static NvAPI_GPU_SetDitherControl_t NvAPI_GPU_SetDitherControl;

    private static ColorSpaceConversion GetColorSpaceConversion(GPUOutput output)
    {
        var displayId = output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId;
        var csc = new Csc { version = 0x1007C };
        var status = NvAPI_GPU_GetColorSpaceConversion(displayId, ref csc);

        if (status != 0)
        {
            throw new Exception("NvAPI_GPU_GetColorSpaceConversion failed with error code " + status);
        }

        var result = new ColorSpaceConversion
        {
            contentColorSpace = csc.contentColorSpace,
            monitorColorSpace = csc.monitorColorSpace
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

    private static void SetColorSpaceConversion(GPUOutput output, ColorSpaceConversion conversion)
    {
        var displayId = output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId;

        var csc = new Csc
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

    public static void SetColorSpaceConversion(GPUOutput output, Matrix matrix) => SetColorSpaceConversion(output, MatrixToColorSpaceConversion(matrix));

    public static unsafe void SetColorSpaceConversion(GPUOutput output, ICCMatrixProfile profile, ColorSpace target, IToneCurve? curve = null)
    {
        var matrix = profile.matrix.Inverse() * Colorimetry.RGBToPCSXYZ(target);

        if (curve == null)
        {
            SetColorSpaceConversion(output, MatrixToColorSpaceConversion(matrix));
            return;
        }

        var displayId = output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId;
        var gamma = new float[2, 1024, 3];
        fixed (float* buffer = gamma)
        {
            var csc = new Csc
            {
                version = 0x200A0,
                contentColorSpace = 2,
                monitorColorSpace = 2,
                degamma = buffer,
                regamma = buffer + 0x3000 / sizeof(float),
                buffer = buffer,
                bufferSize = 0x6000,
            };

            for (var i = 1; i < 1024; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    gamma[0, i, j] = (float)curve.SampleAt(i / 1023d);
                }
            }

            for (var i = 0; i < 3; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    csc.matrix1[i * 4 + j] = (float)matrix[i, j];
                }
            }

            csc.useMatrix1 = 1;

            for (var i = 0; i < 1024; i++)
            {
                for (var j = 0; j < 3; j++)
                {
                    var value = profile.trcs[j].SampleInverseAt(i / 1023d);

                    if (profile.vcgt != null)
                    {
                        value = profile.vcgt[j].SampleAt(value);
                    }

                    gamma[1, i, j] = (float)value;
                }
            }

            var status = NvAPI_GPU_SetColorSpaceConversion(displayId, ref csc);
            if (status != 0)
            {
                throw new Exception("NvAPI_GPU_SetColorSpaceConversion failed with error code " + status);
            }
        }
    }

    public static bool IsColorSpaceConversionActive(GPUOutput output) => GetColorSpaceConversion(output).monitorColorSpace != 0;
    public static void DisableColorSpaceConversion(GPUOutput output) => SetColorSpaceConversion(output, new ColorSpaceConversion { contentColorSpace = 2 });

    public static EDID GetEDID(Display display)
    {
        try
        {
            var displays = WindowsDisplayAPI.Display.GetDisplays();
            var devicePath = displays.First(x => x.DisplayName == display.Name).DevicePath;
            var registryPath = $"HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Enum\\DISPLAY\\{string.Join("\\", devicePath.Split('#').Skip(1).Take(2))}";
            var registryKey = $"{registryPath}\\Device Parameters";
            var registryItem = (byte[]?)Registry.GetValue(registryKey, "EDID", null);
            return new EDID(registryItem);
        }
        catch
        {
            return new EDID(display.Output.PhysicalGPU.ReadEDIDData(display.Output));
        }
    }

    private static ColorSpaceConversion MatrixToColorSpaceConversion(Matrix matrix)
    {
        var csc = new ColorSpaceConversion
        {
            contentColorSpace = 2,
            monitorColorSpace = 2,
            matrix1 = new float[3, 4]
        };

        for (var i = 0; i < 3; i++)
        {
            for (var j = 0; j < 3; j++)
            {
                csc.matrix1[i, j] = (float)matrix[i, j];
            }
        }

        return csc;
    }

    public static DitherControl GetDitherControl(GPUOutput output)
    {
        var dither = new Dither
        { version = 0x10018 };
        var status = NvAPI_GPU_GetDitherControl(output.PhysicalGPU.GetDisplayDeviceByOutput(output).DisplayId,
            ref dither);
        if (status != 0)
        {
            throw new Exception("NvAPI_GPU_GetDitherControl failed with error code " + status);
        }

        return dither.ditherControl;
    }

    public static void SetDitherControl(GPUOutput output, int state, int bits, int mode)
    {
        var status = NvAPI_GPU_SetDitherControl(output.PhysicalGPU.GPUId, (uint)output.OutputId, state, bits, mode);
        if (status != 0)
        {
            throw new Exception("NvAPI_GPU_SetDitherControl failed with error code " + status);
        }
    }

    static Novideo()
    {
        NvAPI_GPU_GetColorSpaceConversion =
            Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_GetColorSpaceConversion_t>(
                NvAPI_QueryInterface(_NvAPI_GPU_GetColorSpaceConversion));
        NvAPI_GPU_SetColorSpaceConversion =
            Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_SetColorSpaceConversion_t>(
                NvAPI_QueryInterface(_NvAPI_GPU_SetColorSpaceConversion));
        NvAPI_GPU_GetDitherControl =
            Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_GetDitherControl_t>(
                NvAPI_QueryInterface(_NvAPI_GPU_GetDitherControl));
        NvAPI_GPU_SetDitherControl =
            Marshal.GetDelegateForFunctionPointer<NvAPI_GPU_SetDitherControl_t>(
                NvAPI_QueryInterface(_NvAPI_GPU_SetDitherControl));
    }
}
