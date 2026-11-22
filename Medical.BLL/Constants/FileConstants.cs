using System.Reflection.Metadata;

namespace Medical.BLL.Constants
{
    public static class FileConstants
    {
        public const long MinSize = 10 * 1024; // 10 KB
        public static long MaxSize = 5* 1024 * 1024; // 5 MB

        public static readonly List<string> AllowedTypes = new List<string>
        {
            "image/png",
            "image/jpeg",
            "image/webp",
            "application/pdf"
        };
    }
}