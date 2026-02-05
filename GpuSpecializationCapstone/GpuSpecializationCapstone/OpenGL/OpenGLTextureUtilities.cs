using Silk.NET.OpenGL;

namespace GpuSpecializationCapstone.OpenGL;

/// <summary>
/// The OpenGL texture utilities.
/// </summary>
public static class OpenGlTextureUtilities
{
    /// <summary>
    /// Creates OpenGL texture.
    /// </summary>
    /// <param name="gl">The <see cref="GL"/> api.</param>
    /// <param name="pixels">The image pixels.</param>
    /// <param name="width">The width of texture.</param>
    /// <param name="height">The height of texture.</param>
    /// <returns>The OpenGL texture.</returns>
    public static unsafe uint Create(GL gl, byte[] pixels, uint width, uint height)
    {
        uint texture =  gl.GenTexture();

        gl.BindTexture(TextureTarget.Texture2D, texture);

        fixed (byte* pixelsPtr = pixels)
        {
            gl.TexImage2D(GLEnum.Texture2D, 0, 
                InternalFormat.Rgba8, width, height, 0, GLEnum.Bgra,
                GLEnum.UnsignedByte, pixelsPtr);
        }
        
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int) GLEnum.Linear);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int) GLEnum.LinearMipmapLinear);
        gl.GenerateMipmap(GLEnum.Texture2D);
        return texture;
    }
}