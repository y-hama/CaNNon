using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Alea.Parallel;

using OpenCvSharp;

using Core.Common;
using Core.Field;
using Core.Process.Function;

namespace Core
{
    public static class Loader
    {
        public static void Start()
        {
            bool usecam = false;
            string folderpath = @"\clothes";

            int batchMax = 5;

            var size = new Size(300, 300);
            var inChannels = 3;
            var outChannels = 3;
            var kernelsize = 3;
            var stride = 2;
            var rho = 1;

            Mat frame = new Mat();

            var totaltime = new Stopwatch();
            var gputime = new Stopwatch();
            double totalelapsed = 0;
            double gpuelapsed = 0;

            using (var gpu = Gpu.Default)
            using (var cap = new VideoCapture(1))
            {
                var conv = Process.Layer.Layer.Load(
                new Process.Property.ConvProperty(
                    gpu,
                    size.Width, size.Height,
                    inChannels, outChannels,
                    kernelsize, stride,
                    rho));

                int count = 0, epoch = 0;
                var files = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + folderpath).GetFiles();
                int index = 0;
                while (true)
                {
                    totaltime.Start();
                    if (usecam)
                    {
                        cap.Read(frame);
                        frame = frame.Flip(FlipMode.Y);
                    }
                    else
                    {
                        index++;
                        if (index >= files.Length) { index = 0; }
                        if (count >= batchMax)
                        {
                            Console.WriteLine($"e:{epoch++}, {conv.Difference}");
                            conv.Update();
                            conv.ShowOutput();

                            count = 0;
                        }
                        count++;
                        frame = new Mat(files[index].FullName);
                    }

                    conv.property.Input.ReadFrom(frame);

                    gputime.Start();
                    conv.Forward();
                    conv.property.Output.DifferenceOf(conv.property.Input, ref conv.property.Sigma);
                    conv.Back();
                    gpuelapsed = gputime.Elapsed();

                    totalelapsed = totaltime.Elapsed();
                    //Console.WriteLine($"g:{gen++}, {totalelapsed}, {gpuelapsed}");

                    //GC.Collect();
                }
            }
        }
    }
}
