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
            var size = new Size(128, 128);
            int batchCount = 3;

            string folderpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\clothes";

            using (var gpu = Gpu.Default)
            {
                var model = new Model.Model(gpu, new Reader.ImageFile(3, folderpath, 0));

                //model.AddLayer(Process.Layer.Layer.Load(
                //        new Process.Property.ConvProperty(
                //            gpu, outChannels: 8,
                //            dilation: 1, expand: 1, kernelSize: 1,
                //            opt: new OptAdaBound() { DropOut = 0 }, act: new ActReLU() { Parameter = 0.01 })));
                model.AddLayer(Process.Layer.Layer.Load(
                    new Process.Property.PoolingProperty(
                        gpu, reduction: 2, expansion: 1
                        )));
                model.AddLayer(Process.Layer.Layer.Load(
                        new Process.Property.ConvProperty(
                            gpu, outChannels: 3,
                            dilation: 1, expand: 1, kernelSize: 1,
                            opt: new OptAdaBound() { DropOut = 0 }, act: new ActReLU() { Parameter = 0.01 })));

                model.Confirm(size);

                int viewcounter = 0;
                while (true)
                {
                    model.Learn(batchCount);
                    Console.WriteLine($"b:{model.Batch}, e:{model.Epoch}, g:{model.Generation}, {model.Difference}");
                    viewcounter++;
                    if (viewcounter % 1 == 0)
                    {
                        model.ModelField.Show("modelfield");
                    }
                }
            }
        }
    }
}
