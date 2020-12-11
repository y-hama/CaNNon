using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Core.Field;

namespace Core.Process.Property
{
    class ConvProperty : Property
    {
        public override Type Connection => typeof(Layer.ConvLayer);

        public double Rho = 0.25;
        public int Stride = 1;

        public KernelField Kernel;
        public KernelField dKernel;

        public ConvProperty(Gpu gpu, int width, int height, int inChannels, int outChannels, int kernelSize, int stride = 1, double rho = 0.25)
            : base(gpu, width, height, inChannels, outChannels)
        {
            Rho = 0.25;
            Stride = stride;
            Kernel = new KernelField(inChannels, outChannels, kernelSize);
            dKernel = new KernelField(inChannels, outChannels, kernelSize);
            Kernel.Randmize();
        }

    }
}
