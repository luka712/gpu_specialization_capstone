using GpuSpecializationCapstone.Texture;

namespace GpuSpecializationCapstone;

public enum DownsizerType
{
    Nearest,
    Linear
}

/// <summary>
/// The interface for image downsizer.
/// </summary>
public interface IImageDownsizer : IDisposable
{
    /// <summary>
    /// Reads the resulting image created by <see cref="Run"/>.
    /// </summary>
    /// <param name="width">The width of image.</param>
    /// <param name="height">The height of image.</param>
    /// <returns>The byte array of pixels from image.</returns>
    byte[] ReadImage(out uint width, out uint height);

    /// <summary>
    /// Setup source image and mipmap level of resize.
    /// </summary>
    /// <param name="level">
    /// The level of resize. Each level is downsized by factor of two, meaning 1st level is divide by 2, second by 4 etc...
    /// </param>
    /// <param name="sourceImage">The <see cref="OpenCLTexture"/> which is source image from which to generate downscaled image.</param>
    void SetupLevel(uint level, OpenCLTexture sourceImage);

    /// <summary>
    /// Runs the downsizer and generate destination image.
    /// </summary>
    void Run();
}