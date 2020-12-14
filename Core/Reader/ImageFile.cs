using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reader
{
    class ImageFile : Reader
    {
        private string SourceLocation { get; set; }

        private System.IO.FileInfo[] files { get; set; }

        private int index { get; set; } = 0;
        private int epoch { get; set; } = 0;

        public ImageFile(string location)
        {
            SourceLocation = location;
        }

        protected override void Initialize()
        {
            files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            index = 0;
        }

        protected override void Get(ref BufferItem buffer)
        {
            buffer.Epoch = epoch;
            buffer.Input.ReadFrom(new OpenCvSharp.Mat(files[index].FullName));
            buffer.Teacher.ReadFrom(new OpenCvSharp.Mat(files[index].FullName));

            index++;
            if (index >= files.Length)
            {
                epoch++;
                index = 0;
                files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            }
        }
    }
}
