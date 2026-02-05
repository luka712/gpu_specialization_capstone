using GpuSpecializationCapstone.OpenGL;
using Silk.NET.OpenGL;

namespace GpuSpecializationCapstone.Texture;

/// <summary>
/// The OpenGL texture.
/// </summary>
public class OpenGlTexture : IDisposable
{
    private readonly GL _gl;
    private readonly byte[] _pixels;
    private bool _initialized;
    private bool _disposed;
    
    /// <summary>
    /// The texture handle.
    /// </summary>
    public uint Handle { get; private set; }
    
    /// <summary>
    /// The width of texture.
    /// </summary>
    public  uint Width { get; }
    
    /// <summary>
    /// The height of texture.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    /// The constructor.
    /// </summary>
    /// <param name="gl">The <see cref="GL"/> api.</param>
    /// <param name="pixels">The texture pixels.</param>
    /// <param name="width">The texture width.</param>
    /// <param name="height">The texture height.</param>
    public OpenGlTexture(GL gl, byte[] pixels, uint width, uint height)
    {
        _gl = gl;
        _pixels = pixels;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initialize the texture.
    /// </summary>
    public void Initialize()
    {
        if (!_initialized)
        {
            Handle = OpenGlTextureUtilities.Create(_gl, _pixels, Width, Height);
            _initialized = true;
        }
    }
    
    /// <summary>
    /// Change mag filter.
    /// </summary>
    /// <param name="magFilter">The <see cref="TextureMagFilter"/>.</param>
    public void ChangeMagFilter(TextureMagFilter magFilter)
    {
        _gl.BindTexture(GLEnum.Texture2D, Handle);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int) magFilter);
    }

    /// <summary>
    /// Change min filter.
    /// </summary>
    /// <param name="minFilter">The <see cref="TextureMinFilter"/>.</param>
    public void ChangeMinFilter(TextureMinFilter minFilter)
    {
        _gl.BindTexture(GLEnum.Texture2D, Handle);
        _gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int) minFilter);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _gl.DeleteTexture(Handle);
            Handle = 0;
            _disposed = true;
        }
    }
}