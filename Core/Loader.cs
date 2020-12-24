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
            var size = new Size(64, 64);
            int batchMax = 2;

            string folderpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\img";

            using (var gpu = Gpu.Default)
            {
                var model = new Model.Model(gpu, new Reader.ImageFile(3, folderpath, 0));

                model.AddLayer(Process.Layer.Layer.Load(
                        new Process.Property.ConvProperty(
                            gpu, outChannels: 3,
                            dilation: 1, expand: 3, kernelSize: 1,
                            new OptAdam())));
                model.AddLayer(Process.Layer.Layer.Load(
                        new Process.Property.ConvProperty(
                            gpu, outChannels: 4,
                            dilation: 1, expand: 1, kernelSize: 1,
                            new OptAdam())));
                model.AddLayer(Process.Layer.Layer.Load(
                        new Process.Property.ConvProperty(
                            gpu, outChannels: 3,
                            dilation: 1, expand: 1, kernelSize: 1,
                            new OptAdam())));

                model.Confirm(size);

                while (true)
                {
                    model.Learn(batchMax);
                    Console.WriteLine($"b:{model.Batch}, e:{model.Epoch}, g:{model.Generation}, {model.Difference}");
                    model.Input.Show("in");
                    //model.Sigma.Show("sigma");
                    //model.HiddenOutput(0).Show("hout-0");
                    model.Teacher.Show("teacher");
                    model.Output.Show("out");
                    BufferField.ShowAllField();
                }
            }
        }
    }
}
