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

        Optimizer optimizer = null;

        public int KernelSize = 0;

        public int Dilation = 1;
        public int Expand = 0;

        public KernelField Kernel;

        public ConvProperty(Gpu gpu, int outChannels, int dilation, int expand, int kernelSize, Optimizer opt)
            : base(gpu, outChannels)
        {
            KernelSize = kernelSize;
            Dilation = dilation;
            Expand = expand;
            optimizer = opt;
        }

        protected override void Adjustment(int inputWidth, int inputHeight, out int outputWidth, out int ouputHeight)
        {
            outputWidth = inputWidth + (inputWidth - 1) * (Expand - 1);
            ouputHeight = inputHeight + (inputHeight - 1) * (Expand - 1);
        }

        protected override void ConfirmField()
        {
            Kernel = new KernelField(inCh, outCh, KernelSize, optimizer);
            Kernel.Randmize();
        }
    }
}
