using Silk.NET.OpenCL;

namespace GpuSpecializationCapstone.OpenCL;

public class OpenCLCommandQueueUtilities
{
    /// <summary>
    /// Create a command queue on the first device available on the context.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The context.</param>
    /// <param name="device">The device.</param>
    /// <returns></returns>
    public static unsafe nint Create(CL cl, nint context, nint device)
    {
        var commandQueue = cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out int errorCode);
        if (commandQueue == IntPtr.Zero)
        {
            Console.WriteLine("Failed to create commandQueue for device 0");
            return IntPtr.Zero;
        }
        return commandQueue;
    }
}