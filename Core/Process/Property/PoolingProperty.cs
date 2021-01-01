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
    class PoolingProperty : Property
    {
        public override Type Connection => typeof(Layer.PoolingLayer);

        public int Reduction = 1;
        public int Expansion = 1;

        public BufferField Map;

        public PoolingProperty(Gpu gpu, int reduction, int expansion)
            : base(gpu, 0)
        {
            Reduction = reduction;
            Expansion = expansion;
        }

        protected override void Adjustment(int inputWidth, int inputHeight, out int outputWidth, out int ouputHeight)
        {
            outCh = inCh;
            outputWidth = (inputWidth / Reduction) * Expansion;
            ouputHeight = (inputHeight / Reduction) * Expansion;
        }

        protected override void ConfirmField()
        {
            Map = new BufferField(GPU, new OpenCvSharp.Size(inW, inH), inCh);

        }
    }
}
