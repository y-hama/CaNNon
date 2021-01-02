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
using Core.Process.Property;

namespace Core
{
    public static class Loader
    {

        private static void CreateLayer(Gpu gpu, ref Model.Model model)
        {
            model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new PoolingProperty(gpu, 2, 1));
            model.AddLayer(new ConvProperty(gpu, 12, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new PoolingProperty(gpu, 2, 1));
            model.AddLayer(new ConvProperty(gpu, 24, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new ConvProperty(gpu, 12, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new PoolingProperty(gpu, 1, 2));
            model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new PoolingProperty(gpu, 1, 2));
            model.AddLayer(new ConvProperty(gpu, 3, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
        }

        public static void Start()
        {
            var size = new Size(64, 64);
            int batchCount = 1;

            string folderpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\img";

            using (var gpu = Gpu.Default)
            {
                var model = new Model.Model(gpu, new Reader.ImageFile(3, folderpath, 0));

                CreateLayer(gpu, ref model);
                model.Confirm(size);

                int viewcounter = 0;
                while (true)
                {
                    model.Learn(batchCount);
                    Console.WriteLine($"b:{model.Batch}, e:{model.Epoch}, g:{model.Generation}, {model.Difference}");
                    viewcounter++;
                    if (viewcounter % 1 == 0)
                    {
                        model.ModelField.Show("modelfield",2);
                    }
                }
            }
        }
    }
}
