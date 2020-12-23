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

        public int Dilation = 1;

        public KernelField Kernel;

        public ConvProperty(Gpu gpu, int inChannels, int outChannels, int dilation, int kernelSize, Optimizer opt)
            : base(gpu, inChannels, outChannels)
        {
            Dilation = dilation;
            Kernel = new KernelField(inChannels, outChannels, kernelSize, opt);
            Kernel.Randmize();
        }

        protected override void Adjustment(int inputWidth, int inputHeight, out int outputWidth, out int ouputHeight)
        {
            outputWidth = inputWidth;
            ouputHeight = inputHeight;
        }
    }
}
