namespace GpuSpecializationCapstone;

public class StringUtilities
{
    /// <summary>
    /// Converts bytes ptr array to string.
    /// </summary>
    public static unsafe string FromBytes(byte* bytes, UIntPtr sizePtr)
    {
        uint size = sizePtr.ToUInt32();
        char[] chars = new char[size];
        for (int i = 0; i < size; i++)
        {
            chars[i] = (char) bytes[i];
        }
        
        return new string(chars);
    }
}