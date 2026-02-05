using Silk.NET.OpenCL;

namespace GpuSpecializationCapstone.OpenCL;

/// <summary>
/// The utility class for working with OpenCL program.
/// </summary>
public class OpenCLProgramUtilities
{
    /// <summary>
    /// Create an OpenCL program from the kernel source.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The OpenCL context pointer.</param>
    /// <param name="device">The OpenCL device pointer.</param>
    /// <param name="kernelSource">The kernel source code.</param>
    /// <returns>The OpenCL program pointer.</returns>
    public static unsafe nint CreateProgram(CL cl, nint context, nint device, string kernelSource)
    {
        nint program = cl.CreateProgramWithSource(context, 1, [kernelSource], null, out int errorCode);
        OpenCLCheckError.CheckError(errorCode);

        errorCode = cl.BuildProgram(program, 0, null, (byte*)null, null, null);
        
        if (errorCode != (int)ErrorCodes.Success)
        {
            _ = cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, 0, null, out nuint buildLogSize);
            byte[] log = new byte[buildLogSize / (nuint)sizeof(byte)];
            fixed (void* pValue = log)
            {
                cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, buildLogSize, pValue, null);
            }

            string? build_log = System.Text.Encoding.UTF8.GetString(log);

            //Console.WriteLine("Error in kernel: ");
            Console.WriteLine("=============== OpenCL Program Build Info ================");
            Console.WriteLine(build_log);
            Console.WriteLine("==========================================================");

            cl.ReleaseProgram(program);
            OpenCLCheckError.CheckError(errorCode);

            return IntPtr.Zero;
        }

        return program;
    }
}