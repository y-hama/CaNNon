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

        protected int inCh { get; set; }
        protected int inW { get; set; }
        protected int inH { get; set; }

        protected int outCh { get; set; }
        protected int outW { get; set; }
        protected int outH { get; set; }

        protected Property(Gpu gpu, int outChannels)
        {
            GPU = gpu;
            outCh = outChannels;
        }

        protected abstract void Adjustment(int inputWidth, int inputHeight, out int outputWidth, out int ouputHeight);

        protected abstract void ConfirmField();

        public void SetInputSize(int width, int height, int inChannels)
        {
            inW = width; inH = height;
            inCh = inChannels;
            int _outW = 0, _outH = 0;

            Adjustment(width, height, out _outW, out _outH);
            outW = _outH; outH = _outH;

            var inS = new OpenCvSharp.Size(inW, inH);
            var outS = new OpenCvSharp.Size(outW, outH);
            Input = new BufferField(GPU, inS, inCh);
            Output = new BufferField(GPU, outS, outCh);
            Sigma = new BufferField(GPU, outS, outCh);
            Propagater = new BufferField(GPU, inS, inCh);

            ConfirmField();
        }

    }
}
