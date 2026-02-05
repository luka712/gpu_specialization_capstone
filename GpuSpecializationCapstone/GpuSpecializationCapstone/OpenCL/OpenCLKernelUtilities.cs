using Silk.NET.OpenCL;

namespace GpuSpecializationCapstone.OpenCL;

/// <summary>
/// The OpenCL kernel utilities.
/// </summary>
public class OpenCLKernelUtilities
{
    /// <summary>
    /// Creates a kernel for a give program
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="program">The OpenCL program.</param>
    /// <param name="kernelName">The kernel name.</param>
    /// <returns>The OpenCL kernel.</returns>
    public static unsafe nint Create(CL cl, nint program, string kernelName)
    {
        nint errorCode = IntPtr.Zero;
        nint kernel = cl.CreateKernel(program, kernelName, (int*) errorCode);
        
        if (errorCode != (int) ErrorCodes.Success)
        {
            Console.WriteLine("Failed to create kernel");
            return IntPtr.Zero;
        }
        return kernel;
    }
}