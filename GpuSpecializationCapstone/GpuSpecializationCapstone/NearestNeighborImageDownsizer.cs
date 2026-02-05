using GpuSpecializationCapstone.OpenCL;
using GpuSpecializationCapstone.Texture;
using Silk.NET.OpenCL;
using static GpuSpecializationCapstone.OpenCL.OpenCLCheckError;

namespace GpuSpecializationCapstone;

/// <summary>
/// The Nearest-Neighbor implementation of downsizer.
/// </summary>
public class NearestNeighborImageDownsizer : IImageDownsizer
{
    private const string PROGRAM_SOURCE_CODE = @"
    __constant sampler_t kNearestSampler =
        CLK_NORMALIZED_COORDS_FALSE |
        CLK_ADDRESS_CLAMP |
        CLK_FILTER_NEAREST;

    __kernel __attribute__((reqd_work_group_size(8, 8, 1)))
    void downsample_nn(
        __read_only  image2d_t src,
        __write_only image2d_t dst,
        int step
    ) {
        int2 gid = (int2)(get_global_id(0), get_global_id(1));
        int2 srcCoord = gid * step;
        
        int2 srcSize = (int2)(
            get_image_width(src),
            get_image_height(src)
        );
        
        int2 dstSize = (int2)(
            get_image_width(dst),
            get_image_height(dst)
        );
        
        if (gid.x >= dstSize.x || gid.y >= dstSize.y ||
            srcCoord.x >= srcSize.x || srcCoord.y >= srcSize.y)
            return;
        
        float4 color = read_imagef(src, kNearestSampler, srcCoord);
        write_imagef(dst, gid, color);
    }";
    
    private readonly CL _cl;
    private readonly nint _context;
    private readonly nint _queue;
    private readonly nint _program;
    private readonly nint _kernel;
    
    private OpenCLTexture? _sourceImage;
    private OpenCLTexture? _destinationImage;
    
    private int _level;
    private uint _step;
    
    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The context pointer.</param>
    /// <param name="device">The device pointer.</param>
    /// <param name="queue">The command queue pointer.</param>
    public NearestNeighborImageDownsizer(CL cl, nint context, nint device, nint queue)
    {
        _cl = cl;
        _context = context;
        _queue = queue;
        _program = OpenCLProgramUtilities.CreateProgram(cl, context, device, PROGRAM_SOURCE_CODE);
        _kernel = OpenCLKernelUtilities.Create(cl, _program, "downsample_nn");
    }

    /// <inheritdoc/>
    public void SetupLevel(uint level, OpenCLTexture sourceImage)
    {
        _level = (int)level;
        _sourceImage = sourceImage;
        _step = (uint) Math.Pow(2, _level);
        
        _destinationImage = new OpenCLTexture(_cl, _context, _queue, sourceImage.Width / _step, sourceImage.Height / _step);
        _destinationImage.Initialize();
    }

    /// <inheritdoc/>
    public unsafe void Run()
    {
        if (_sourceImage is null || _destinationImage is null)
        {
            throw new InvalidOperationException(
                $"{nameof(NearestNeighborImageDownsizer)} requires a source image. Call {nameof(SetupLevel)}() first.");
        }
        
        int step = (int) _step;
        IntPtr sourceImagePtr = new  IntPtr(_sourceImage.Handle);
        IntPtr destinationImagePtr = new  IntPtr(_destinationImage.Handle);
        CheckError(_cl.SetKernelArg(_kernel, 0u, (uint) sizeof(void*), ref sourceImagePtr));
        CheckError(_cl.SetKernelArg(_kernel, 1u, (uint) sizeof(void*), ref destinationImagePtr));
        CheckError(_cl.SetKernelArg(_kernel, 2u, sizeof(int), &step));
        
        UIntPtr* localSize = stackalloc UIntPtr[2] { new UIntPtr(8), new  UIntPtr(8) };
        UIntPtr* globalSize = stackalloc UIntPtr[2]
        {
            new UIntPtr((uint) ((_destinationImage.Width + 7) & ~7)),
            new UIntPtr((uint) ((_destinationImage.Height + 7) & ~7))
        };
        
        int error = _cl.EnqueueNdrangeKernel(_queue, _kernel, 2, null, 
            globalSize, localSize,
            0, null, null);
        CheckError(error);
    }

    /// <inheritdoc/>
    public byte[] ReadImage(out uint width, out uint height)
    {
        if (_destinationImage is null)
        {
            throw new InvalidOperationException($"Destination image is null. Make sure to call {nameof(NearestNeighborImageDownsizer)}.{nameof(Run)}().");
        }
        
        width = _destinationImage.Width;
        height = _destinationImage.Height;
        return OpenClImageUtilities.ReadImage(_cl,_queue, _destinationImage.Handle, width, height);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cl.ReleaseKernel(_kernel);
        _cl.ReleaseProgram(_program);
        _destinationImage?.Dispose();
    }
}