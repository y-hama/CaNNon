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

            Mat frame = new Mat();

            var totaltime = new Stopwatch();
            var gputime = new Stopwatch();
            double totalelapsed = 0;
            double gpuelapsed = 0;

            using (var gpu = Gpu.Default)
            using (var cap = new VideoCapture(1))
            {
                int layercount = 6;
                var list = new List<Process.Layer.Layer>();
                for (int i = 0; i < layercount; i++)
                {
                    list.Add(Process.Layer.Layer.Load(
                            new Process.Property.ConvProperty(
                                gpu,
                                size.Width, size.Height,
                                inChannels, outChannels,
                                stride, kernelsize,
                                new OptAdam())));
                }
                for (int i = 0; i < layercount - 1; i++)
                {
                    Process.Layer.Layer.Connect(list[i], list[i + 1]);
                }

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
                        Console.WriteLine($"g:{gen}, e:{epoch++}, {list[list.Count - 1].Difference}");
                        for (int i = 0; i < layercount; i++)
                        {
                            list[i].Update();
                            list[i].ShowOutput($"{i}");
                        }
                        BufferField.ShowAllField();

                        count = 0;
                        GC.Collect();
                    }
                    count++;

                    list[0].property.Input.ReadFrom(frame);

                    gputime.Start();
                    for (int i = 0; i < layercount; i++)
                    {
                        list[i].Forward();
                    }
                    list[list.Count - 1].property.Output.DifferenceOf(list[0].property.Input, ref list[list.Count - 1].property.Sigma);
                    for (int i = layercount - 1; i >= 0; i--)
                    {
                        list[i].Back();
                    }
                    gpuelapsed = gputime.Elapsed();

                    totalelapsed = totaltime.Elapsed();
                    //Console.WriteLine($"g:{gen++}, {totalelapsed}, {gpuelapsed}");

                }
            }
        }
    }
}
