using Silk.NET.OpenCL;
using static GpuSpecializationCapstone.OpenCL.OpenCLCheckError;
using static GpuSpecializationCapstone.StringUtilities;

namespace GpuSpecializationCapstone.OpenCL;

/// <summary>
/// The utilities for working with OpenCL context.
/// </summary>
public class OpenCLContextUtilities
{
    /// <summary>
    /// Create an OpenCL context on the first available platform using
    /// either a GPU or CPU depending on what is available.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <returns>The context pointer.</returns>
    public static unsafe nint Create(CL cl)
    {
        // Get platforms count.
        CheckError(cl.GetPlatformIDs(0, null, out uint numPlatformIds));

        // Get platforms.
        nint* platformIds = stackalloc nint[(int) numPlatformIds];
        CheckError(cl.GetPlatformIDs(numPlatformIds, platformIds, out uint _));

        byte* bufferPtr = stackalloc byte[1024];
        uint* sizePtr = stackalloc uint[1];
        sizePtr[0] = 1024;
        for (int i = 0; i < numPlatformIds; i++)
        {
            nint platformId = platformIds[i];
            cl.GetPlatformInfo(platformId, PlatformInfo.Name, (UIntPtr)sizePtr, bufferPtr, out UIntPtr sizeUsed);
            string name = FromBytes(bufferPtr, sizeUsed);

            cl.GetPlatformInfo(platformId, PlatformInfo.Name, (UIntPtr)sizePtr, bufferPtr, out sizeUsed);
            string vendor = FromBytes(bufferPtr, sizeUsed);
            
            // Next, create an OpenCL context on the platform.  Attempt to
            // create a GPU-based context, and if that fails, try to create
            // a CPU-based context.
            nint[] contextProperties =
            [
                (nint)ContextProperties.Platform,
                platformId,
                0
            ];

            fixed (nint* p = contextProperties)
            {
                var context = cl.CreateContextFromType(p, DeviceType.Gpu, null, null, out int errNum);
                if (errNum != (int)ErrorCodes.Success)
                {
                    Console.WriteLine("Could not create GPU context, trying CPU...");

                    context = cl.CreateContextFromType(p, DeviceType.Cpu, null, null, out errNum);

                    CheckError(errNum);
                    
                    return context;
                }

                if (context != IntPtr.Zero)
                {
                    Console.WriteLine("Context name: " + name);
                    return  context;
                }
            }
        }
        return IntPtr.Zero;
    }
}