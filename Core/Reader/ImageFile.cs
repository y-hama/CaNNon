﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reader
{
    class ImageFile : Reader
    {
        public double RandomNoize { get; set; } = 0;

        protected override int BufferingSize { get => 10; }

        private string SourceLocation { get; set; }

        private System.IO.FileInfo[] files { get; set; }

        private int index1 { get; set; } = 0;
        private int index2 { get; set; } = 0;

        private Random random { get; set; } = new Random();
        private List<int> sellection { get; set; } = new List<int>();

        public ImageFile(int readChannels, string location)
            : base(readChannels)
        {
            SourceLocation = Adjust.ImageFileAdjust.AdjustedLocation(location);
        }

        protected override void Initialize()
        {
            files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
            index1 = index2 = 0;
        }

        protected override void Get(ref BufferItem buffer)
        {
            buffer.Epoch = epoch;

            var frame1 = new OpenCvSharp.Mat(files[index1].FullName);
            buffer.Input.ReadFrom(frame1);
            if (RandomNoize != 0)
            {
                RandomNoize(ref buffer.Input, RandomNoize, random);
            }

            var frame2 = new OpenCvSharp.Mat(files[index2].FullName);
            buffer.Teacher.ReadFrom(frame2);

            sellection.Add(index1);
            if (sellection.Count >= files.Length)
            {
                epoch++;
                sellection.Clear();
                files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
                index1 = random.Next(0, files.Length);
            }
            else
            {
                while (sellection.Contains(index1))
                {
                    index1 = random.Next(0, files.Length);
                }
            }
            index2 = index1;
            if (index2 >= files.Length)
            {
                index2 = 0;
            }
        }

    }
}
