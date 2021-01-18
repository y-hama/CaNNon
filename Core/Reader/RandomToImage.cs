using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Reader
{
    class RandomToImage : Reader
    {
        protected override int BufferingSize { get => 10; }

        private string SourceLocation { get; set; }

        private Random random { get; set; } = new Random();
        private List<int> sellection { get; set; } = new List<int>();

        private System.IO.FileInfo[] files { get; set; }

        private int index1 { get; set; } = 0;

        public RandomToImage(int readChannels, string location)
            : base(readChannels)
        {
            SourceLocation = Adjust.ImageFileAdjust.AdjustedLocation(location);
        }

        protected override void Initialize()
        {
            files = (new System.IO.DirectoryInfo(SourceLocation)).GetFiles();
        }

        protected override void Get(ref BufferItem buffer)
        {
            buffer.Epoch = epoch;

            for (int c = 0; c < buffer.Input.Channels; c++)
            {
                for (int i = 0; i < buffer.Input.Width; i++)
                {
                    for (int j = 0; j < buffer.Input.Height; j++)
                    {
                        buffer.Input.Buffer[c][i, j] = random.NextDouble();
                    }
                }
            }

            var frame2 = new OpenCvSharp.Mat(files[index1].FullName);
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
        }
    }
}
