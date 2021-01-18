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
    [GpuManaged]
    public static class Loader
    {
        public static string ImageLocation { get; set; } = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\img";

        private static void CreateLayer(Gpu gpu, ref Model.Model model)
        {
            //model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            //model.AddLayer(new PoolingProperty(gpu, 2, 1) { Type = PoolingProperty.PoolType.Max });
            ////model.AddLayer(new ConvProperty(gpu, 12, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            ////model.AddLayer(new PoolingProperty(gpu, 2, 1) { Type = PoolingProperty.PoolType.Max });
            ////model.AddLayer(new ConvProperty(gpu, 24, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            ////model.AddLayer(new PoolingProperty(gpu, 2, 1) { Type = PoolingProperty.PoolType.Max });

            //model.AddLayer(new ConvProperty(gpu, 12, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));

            ////model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Max });
            ////model.AddLayer(new ConvProperty(gpu, 12, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            ////model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Max });
            //model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            //model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Max });



            //model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 2, new OptAdaBound(), new ActReLU() { Parameter = 0 }));
            //model.AddLayer(new ConvProperty(gpu, 5, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            //model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Average });
            //model.AddLayer(new ConvProperty(gpu, 5, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            //model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Average });

            model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 1, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 2, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            model.AddLayer(new ConvProperty(gpu, 6, 1, 1, 2, new OptAdaBound(), new ActReLU() { Parameter = 0.01 }));
            //model.AddLayer(new PoolingProperty(gpu, 1, 2) { Type = PoolingProperty.PoolType.Max });
            model.AddLayer(new ConvProperty(gpu, 3, 1, 1, 1, new OptAdaBound(), new ActReLU()));
        }

        public static void Start()
        {

            var size = new Size(64, 64);
            int batchCount = 1;

            using (var gpu = Gpu.Default)
            {
                var model = new Model.Model(gpu, new Reader.ImageFile(3, ImageLocation) { RandomNoize = 0.3 });
                model.InferenceReader = new Reader.Capture(3, 1);

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
                        //model.Inference();
                        model.ModelField.Show("modelfield", 2);
                    }
                }
            }
        }
    }
}
