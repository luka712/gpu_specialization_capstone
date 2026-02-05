using System.Collections.ObjectModel;
using Silk.NET.OpenCL;
using static GpuSpecializationCapstone.OpenCL.OpenCLCheckError;
using static GpuSpecializationCapstone.StringUtilities;
namespace GpuSpecializationCapstone.OpenCL;

/// <summary>
/// The OpenCL utilities for working with device/s.
/// </summary>
public class OpenCLDeviceUtilities
{
    /// <summary>
    /// Gets a first available device.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The context.</param>
    /// <returns>The device pointer.</returns>
    public static unsafe nint Get(CL cl, nint context)
    {
        CheckError(cl.GetContextInfo(context, ContextInfo.Devices, 0, null, out nuint deviceBufferSize));
        
        if (deviceBufferSize <= 0)
        {
            Console.WriteLine("No devices available.");
            return IntPtr.Zero;
        }

        nint[] devices = new nint[deviceBufferSize / (nuint) sizeof(nuint)];
        byte* bufferPtr = stackalloc byte[1024];
        uint* sizePtr = stackalloc uint[1];
        sizePtr[0] = 1024;
        fixed (void* pValue = devices)
        {
            CheckError(cl.GetContextInfo(context, ContextInfo.Devices, deviceBufferSize, pValue, null));
            
            cl.GetDeviceInfo(devices[0], DeviceInfo.Name, (UIntPtr)sizePtr, bufferPtr, out UIntPtr usedSize);
            string name = FromBytes(bufferPtr, usedSize);

            Console.WriteLine($"OpenCL Device name: {name}");
        }

        nint device = devices[0];
        return device;
    }

    /// <summary>
    /// Gets the supported extensions as string. Extensions are separated by space.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="device">The OpenCL device.</param>
    /// <returns>List of extensions separated by space.</returns>
    public static unsafe string GetExtensions(CL cl, nint device)
    {
        cl.GetDeviceInfo(device, DeviceInfo.Extensions, 0, null, out UIntPtr extSize);
        int size = (int) extSize.ToUInt32();
        
        
        byte* extPtr = stackalloc byte[size];
        cl.GetDeviceInfo(device, DeviceInfo.Extensions, extSize, extPtr, null);
        
        string extensions = FromBytes(extPtr, extSize);

        return extensions;
    }
}