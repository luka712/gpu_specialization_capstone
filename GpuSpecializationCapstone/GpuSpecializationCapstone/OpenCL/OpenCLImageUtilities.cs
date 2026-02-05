using Silk.NET.OpenCL;
using static GpuSpecializationCapstone.OpenCL.OpenCLCheckError;

namespace GpuSpecializationCapstone.OpenCL;

/// <summary>
/// The utility class for working with OpenCL image.
/// </summary>
public class OpenClImageUtilities
{
    /// <summary>
    /// Create an OpenCL image.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The pointer to CL context.</param>
    /// <param name="width">The image width.</param>
    /// <param name="height">The image height.</param>
    /// <param name="memFlags">Teh <see cref="MemFlags"/>. By default it is <see cref="MemFlags.ReadWrite"/>.</param>
    /// <returns>Point to OpenCL image.</returns>
    public static unsafe IntPtr Create(CL cl, nint context, uint width, uint height, MemFlags memFlags = MemFlags.ReadWrite)
    {
        // Create the image
        ImageFormat imageFormat = new(ChannelOrder.Rgba, ChannelType.UnsignedInt8);
        ImageDesc imageDesc = new()
        {
            ImageType = MemObjectType.Image2D,
            ImageWidth = width,
            ImageHeight = height,
            ImageDepth = 0,
            ImageArraySize = 0,
            ImageRowPitch = 4 * width,
            ImageSlicePitch = 0,
            NumMipLevels = 0,
            NumSamples = 0,
            MemObject = 0,
        };
        nint image = cl.CreateImage(context,
            memFlags,
            in imageFormat, in imageDesc,
            null, out int error);

        CheckError(error);

        return image;
    }

    public static unsafe nint Create(CL cl, nint context, byte[] bytes, uint width, uint height, MemFlags memFlags = MemFlags.ReadWrite)
    {
        // Create the image
        ImageFormat imageFormat = new(ChannelOrder.Rgba, ChannelType.UnsignedInt8);
        ImageDesc imageDesc = new()
        {
            ImageType = MemObjectType.Image2D,
            ImageWidth = width,
            ImageHeight = height,
            ImageDepth = 0,
            ImageArraySize = 0,
            ImageRowPitch = 0,
            ImageSlicePitch = 0,
            NumMipLevels = 0,
            NumSamples = 0,
            MemObject = 0,
        };

        fixed (byte* dataPtr = bytes)
        {
            nint image = cl.CreateImage(context,
                memFlags | MemFlags.CopyHostPtr,
                in imageFormat, in imageDesc,
                dataPtr, out int error);
            CheckError(error);
            return image;
        }
    }

    public static unsafe byte[] ReadImage(CL cl, nint queue, nint image, uint width, uint height)
    {
        // Define origin and region for full image
        nuint* origin = stackalloc nuint[3] { 0, 0, 0 };

        nuint* region = stackalloc nuint[3] { width, height, 1 };

        byte[] data = new byte[width * height * 4];

        fixed (byte* dataPtr = data)
        {
            cl.EnqueueReadImage(
                queue, image, true, origin, region, 0, 0, dataPtr, 0, null, null);
        }
        
        return data;
    }
}