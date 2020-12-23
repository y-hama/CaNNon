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

        private int index1 { get; set; } = 0;
        private int index2 { get; set; } = 0;
        private int IndexOffset { get; set; }

        public ImageFile(string location, int indexoffset = 0)
        {
            SourceLocation = location;
            IndexOffset = indexoffset;
        }

        protected override void Initialize()
        {
            files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            index1 = 0;
            index2 = IndexOffset;
        }

        protected override void Get(ref BufferItem buffer)
        {
            buffer.Epoch = epoch;
            var frame1 = new OpenCvSharp.Mat(files[index1].FullName);
            buffer.Input.ReadFrom(frame1);

            var frame2 = new OpenCvSharp.Mat(files[index2].FullName);
            //OpenCvSharp.Cv2.Laplacian(frame2, frame2, frame2.Type(), 1);
            buffer.Teacher.ReadFrom(frame2);

            index1++;
            index2++;
            if (index1 >= files.Length)
            {
                epoch++;
                index1 = 0;
                files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            }
            if (index2 >= files.Length)
            {
                index2 = 0;
            }

        }
    }
}
