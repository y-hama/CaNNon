﻿using System;
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
            buffer.Input.ReadFrom(new OpenCvSharp.Mat(files[index1].FullName));
            buffer.Teacher.ReadFrom(new OpenCvSharp.Mat(files[index2].FullName));

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
