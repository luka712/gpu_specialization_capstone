using GpuSpecializationCapstone.OpenCL;
using GpuSpecializationCapstone.Texture;
using Silk.NET.OpenCL;
using SkiaSharp;

namespace GpuSpecializationCapstone;

public class MipMapGenerator
{
    private readonly CL _cl;
    private readonly nint _context;
    private readonly nint _device;
    private readonly nint _queue;
    private OpenCLTexture _sourceImage;
    private uint _sourceWidth, _sourceHeight;
    private string _lastSource;

    private readonly Dictionary<int, IImageDownsizer> _imageDownsizers = new();

    /// <summary>
    /// Invoked when downsize value has changed.
    /// </summary>
    public event Action OnRunFinished;

    /// <summary>
    /// The constructor.
    /// </summary>
    public MipMapGenerator()
    {
        _cl = CL.GetApi();
        _context = OpenCLContextUtilities.Create(_cl);
        _device = OpenCLDeviceUtilities.Get(_cl, _context);
        _queue = OpenCLCommandQueueUtilities.Create(_cl, _context, _device);
    }

    /// <summary>
    /// The mipmap levels.
    /// </summary>
    public int Levels { get; private set; }

    /// <summary>
    /// The type of downsizer.
    /// </summary>
    public DownsizerType DownsizerType
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                SetSource(_lastSource);
                Run();
            }
        }
    }

    /// <summary>
    /// Sets the source image from the path.
    /// </summary>
    /// <param name="path">The file path.</param>
    public void SetSource(string path)
    {
        _lastSource = path;
        using SKBitmap bitmap = SKBitmap.Decode(path);
        byte[] pixels = bitmap.Bytes;
        _sourceWidth = (uint)bitmap.Width;
        _sourceHeight = (uint)bitmap.Height;

        _sourceImage = new OpenCLTexture(_cl, _context, _queue, pixels, _sourceWidth, _sourceHeight);
        _sourceImage.Initialize();

        int i = 0;
        uint targetWidth = _sourceWidth;
        uint targetHeight = _sourceHeight;
        while (targetWidth > 1 && targetHeight > 1)
        {
            i++;
            targetWidth /= 2;
            targetHeight /= 2;

            if (_imageDownsizers.ContainsKey(i))
            {
                _imageDownsizers[i].Dispose();
            }

            if (DownsizerType == DownsizerType.Linear)
            {
                _imageDownsizers[i] = new LinearImageDownsizer(_cl, _context, _device, _queue);
            }
            else if (DownsizerType == DownsizerType.Nearest)
            {
                _imageDownsizers[i] = new NearestNeighborImageDownsizer(_cl, _context, _device, _queue);
            }

            IImageDownsizer downsizer = _imageDownsizers[i];
            downsizer.SetupLevel((uint)i, _sourceImage);
        }

        Levels = i;
    }

    /// <summary>
    /// Runs and creates downsized textures.
    /// </summary>
    public void Run()
    {
        foreach (IImageDownsizer downsizer in _imageDownsizers.Values)
        {
            downsizer.Run();
        }

        _cl.Finish(_queue);

        OnRunFinished?.Invoke();
    }

    public byte[] ReadImage(int level, out uint width, out uint height)
    {
        if (level == 0)
        {
            width = _sourceWidth;
            height = _sourceHeight;
            return OpenClImageUtilities.ReadImage(_cl, _queue, _sourceImage.Handle, _sourceWidth, _sourceHeight);
        }

        var downsizer = _imageDownsizers[level];
        return downsizer.ReadImage(out width, out height);
    }
}