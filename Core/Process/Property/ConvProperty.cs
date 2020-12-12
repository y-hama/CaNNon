using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Core.Field;
using Core.Process.Function;

namespace Core.Process.Property
{
    class ConvProperty : Property
    {
        public override Type Connection => typeof(Layer.ConvLayer);

        public int Stride = 1;

        public KernelField Kernel;

        public ConvProperty(Gpu gpu, int width, int height, int inChannels, int outChannels, int stride, int kernelSize, Optimizer opt)
            : base(gpu, width, height, inChannels, outChannels)
        {
            Stride = stride;
            Kernel = new KernelField(inChannels, outChannels, kernelSize, opt);
            Kernel.Randmize();
        }

    }
}
