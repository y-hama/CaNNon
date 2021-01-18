using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reader.Adjust
{
    public static class ImageFileAdjust
    {
        private const string AdjustedDirectory = "_ad";

        public static string AdjustedLocation(string source)
        {
            if (System.IO.Directory.Exists(source + AdjustedDirectory))
            {
                return source + AdjustedDirectory;
            }
            return source;
        }

        public static void Start(string source, int size)
        {
            var dst = source + AdjustedDirectory;
            if (System.IO.Directory.Exists(dst))
            {
                System.IO.Directory.Delete(dst, true);
            }
            System.IO.Directory.CreateDirectory(dst);
            foreach (var item in (new System.IO.DirectoryInfo(source).GetFiles()))
            {
                try
                {
                    var frame = new OpenCvSharp.Mat(item.FullName);
                    var ax = Math.Sqrt(frame.Width * frame.Width + frame.Height * frame.Height);
                    if (size < ax)
                    {
                        frame = frame.Resize(new OpenCvSharp.Size(), size / ax, size / ax, OpenCvSharp.InterpolationFlags.Area);
                    }
                    frame.SaveImage($@"{dst}/{item.Name}");
                }
                catch (Exception)
                {
                }
            }
        }

    }
}
