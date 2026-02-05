using Silk.NET.OpenCL;

namespace GpuSpecializationCapstone.OpenCL;

public class OpenCLCheckError
{
    /// <summary>
    /// Check OpenCL error.
    /// </summary>
    /// <param name="error">The error integer.</param>
    /// <exception cref="Exception">If error has occured.</exception>
    public static void CheckError(int error)
    {
        ErrorCodes errorCode = (ErrorCodes)error;
        if (errorCode != (int)ErrorCodes.Success)
        {
            Console.WriteLine("Error code: " + errorCode);
            throw new Exception(errorCode.ToString());
        }
    }
}