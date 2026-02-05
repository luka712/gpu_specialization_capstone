using GpuSpecializationCapstone.OpenCL;
using ImGuiNET;
using Silk.NET.OpenCL;
using Silk.NET.OpenGL;

namespace GpuSpecializationCapstone.Texture;

public class OpenCLTexture : IDisposable
{
    private readonly CL _cl;
    private readonly nint _context, _queue;
    private readonly byte[]? _pixels;

    private bool _initialized;
    private bool _disposed;

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The OpenCL context.</param>
    /// <param name="queue">The OpenCL command queue.</param>
    /// <param name="pixels">The pixels data.</param>
    /// <param name="width">The width of image.</param>
    /// <param name="height">The height of image.</param>
    public OpenCLTexture(CL cl, nint context, nint queue, byte[] pixels, uint width, uint height)
        : this(cl, context, queue, width, height)
    {
        _pixels = pixels;
    }

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="cl">The <see cref="CL"/> api.</param>
    /// <param name="context">The OpenCL context.</param>
    /// <param name="queue">The OpenCL command queue.</param>
    /// <param name="width">The width of image.</param>
    /// <param name="height">The height of image.</param>
    public OpenCLTexture(CL cl, nint context, nint queue, uint width, uint height)
    {
        _cl = cl;
        _context = context;
        _queue = queue;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// The OpenCL image object.
    /// </summary>
    public nint Handle { get; private set; }

    /// <summary>
    /// The image width.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    /// The image height.
    /// </summary>
    public uint Height { get; }


    /// <summary>
    /// Initialize the OpenCL image.
    /// </summary>
    public void Initialize()
    {
        if (!_initialized)
        {
            if (_pixels != null)
            {
                Handle = OpenClImageUtilities.Create(
                    _cl, _context,
                    _pixels, Width, Height,
                    MemFlags.ReadOnly);
                _initialized = true;
            }
            else
            {
                Handle = OpenClImageUtilities.Create(_cl, _context, Width, Height, MemFlags.WriteOnly);
            }
        }
    }
    
    /// <summary>
    /// Reads the image and copies data to returned byte array.
    /// </summary>
    /// <returns>The byte array.</returns>
    public byte[] ReadImage()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"{nameof(OpenCLTexture)} is  not initialized");
        }

        if (_disposed)
        {
            throw new ObjectDisposedException($"{nameof(OpenCLTexture)} is disposed");
        }
        
        return OpenClImageUtilities.ReadImage(_cl,_queue, Handle, Width, Height);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _cl.ReleaseMemObject(Handle);
            Handle = IntPtr.Zero;
            _disposed = true;
        }
    }
}