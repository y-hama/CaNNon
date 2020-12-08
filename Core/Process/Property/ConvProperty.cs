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

        public KernelField Kernel;
        public KernelField dKernel;

        public ConvProperty(Gpu gpu, int width, int height, int inChannels, int outChannels, int kernelSize)
            : base(gpu, width, height, inChannels, outChannels)
        {
            Kernel = new KernelField(inChannels, outChannels, kernelSize);
            dKernel = new KernelField(inChannels, outChannels, kernelSize);
            Kernel.Randmize();
        }

    }
}
