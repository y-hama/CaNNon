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

        protected Property(Gpu gpu, int width, int height, int inChannels, int outChannels)
        {
            GPU = gpu;

            var size = new OpenCvSharp.Size(width, height);
            Input = new BufferField(gpu, size, inChannels);
            Output = new BufferField(gpu, size, outChannels);
            Sigma = new BufferField(gpu, size, outChannels);
            Propagater = new BufferField(gpu, size, inChannels);
        }
    }
}
