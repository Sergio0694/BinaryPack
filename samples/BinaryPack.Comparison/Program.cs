using BinaryPack.Comparison.Implementation;
using BinaryPack.Models;

namespace BinaryPack.Comparison
{
    class Program
    {
        static void Main()
        {
            /* ==========================
             * Binary file comparison
             * ==========================
             * This program will create a new model and populate it, then serialize
             * it both in JSON format and with BinaryPack. It will then print
             * the size in bytes of the resulting serialized data, both before
             * and after a compression with gzip. */
            FileSizeComparer.Run<JsonResponseModel>();
        }
    }
}
