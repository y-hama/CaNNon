using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Core.Field;

namespace Core.Process.Property
{
    abstract class Property
    {
        public abstract Type Connection { get; }

        public Gpu GPU { get; private set; }

        public BufferField Input;
        public BufferField Output;
        public BufferField Sigma;
        public BufferField Propagater;

        private int inCh { get; set; }
        private int outCh { get; set; }
        private int inW { get; set; }
        private int inH { get; set; }

        protected Property(Gpu gpu, int inChannels, int outChannels)
        {
            GPU = gpu;
            inCh = inChannels;
            outCh = outChannels;
        }

        protected abstract void Adjustment(int inputWidth, int inputHeight, out int outputWidth, out int ouputHeight);

        public void SetInputSize(int width, int height)
        {
            inW = width; inH = height;
            int outW = 0, outH = 0;

            Adjustment(width, height, out outW, out outH);

            var inS = new OpenCvSharp.Size(inW, inH);
            var outS = new OpenCvSharp.Size(outW, outH);
            Input = new BufferField(GPU, inS, inCh);
            Output = new BufferField(GPU, outS, outCh);
            Sigma = new BufferField(GPU, outS, outCh);
            Propagater = new BufferField(GPU, inS, inCh);
        }

    }
}
