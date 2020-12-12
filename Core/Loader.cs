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

            int batchMax = 3;

            var size = new Size(256, 160);
            var inChannels = 3;
            var outChannels = 3;
            var kernelsize = 1;
            var stride = 1;
            var rho = 0.1;

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
                    stride, kernelsize,
                    new OptSGD(rho)));

                int gen = 0, count = 0, epoch = 0;
                System.IO.FileInfo[] files = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + folderpath).GetFiles();
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
                        frame = new Mat(files[index].FullName);
                    }

                    index++;
                    if (index >= files.Length)
                    {
                        index = 0;
                        gen++;
                        files = new System.IO.DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + folderpath).GetFiles();
                    }
                    if (count >= batchMax)
                    {
                        Console.WriteLine($"g:{gen}, e:{epoch++}, {conv.Difference}");
                        conv.Update();
                        conv.ShowOutput();

                        count = 0;
                        GC.Collect();
                    }
                    count++;

                    conv.property.Input.ReadFrom(frame);

                    gputime.Start();
                    conv.Forward();
                    conv.property.Output.DifferenceOf(conv.property.Input, ref conv.property.Sigma);
                    conv.Back();
                    gpuelapsed = gputime.Elapsed();

                    totalelapsed = totaltime.Elapsed();
                    //Console.WriteLine($"g:{gen++}, {totalelapsed}, {gpuelapsed}");

                }
            }
        }
    }
}
