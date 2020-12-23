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
            var size = new Size(200, 200);
            var inChannels = 3;
            var outChannels = 3;
            var kernelsize = 1;
            var dilation = 1;

            int layercount = 2;
            int batchMax = 2;

            string folderpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\clothes";

            using (var gpu = Gpu.Default)
            {
                var model = new Model.Model(gpu, new Reader.ImageFile(folderpath, 0));
                for (int i = 0; i < layercount; i++)
                {
                    model.AddLayer(Process.Layer.Layer.Load(
                            new Process.Property.ConvProperty(
                                gpu,
                                inChannels, outChannels,
                                dilation, kernelsize,
                                new OptAdam())));
                }
                model.Confirm(size);

                while (true)
                {
                    model.Learn(batchMax);
                    Console.WriteLine($"b:{model.Batch}, e:{model.Epoch}, g:{model.Generation}, {model.Difference}");
                    model.Input.Show("in");
                    model.Output.Show("out");
                    model.Sigma.Show("sigma");
                    model.HiddenOutput(0).Show("hout-0");
                    BufferField.ShowAllField();
                }
            }
        }
    }
}
