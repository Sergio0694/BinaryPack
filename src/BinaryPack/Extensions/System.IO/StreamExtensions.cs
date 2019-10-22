using System.IO;

namespace System
{
    /// <summary>
    /// A <see langword="class"/> that provides extension methods for the <see cref="Stream"/> type
    /// </summary>
    internal static class StreamExtensions
    {
        /// <summary>
        /// Copies the contents of a given <see cref="Stream"/> to the target <see cref="byte"/> array
        /// </summary>
        /// <param name="stream">The input <see cref="Stream"/> to read data from</param>
        /// <param name="buffer">The target <see cref="byte"/> array to write data to</param>
        /// <remarks>The target array must have enough space to fit the content of the input <see cref="Stream"/></remarks>
        public static void CopyTo(this Stream stream, byte[] buffer)
        {
            using Stream destination = new MemoryStream(buffer, 0, (int)stream.Length);

            stream.CopyTo(destination);
        }
    }
}

