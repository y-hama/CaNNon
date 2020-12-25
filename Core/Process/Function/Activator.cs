using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Field;

using Alea;
using Alea.Parallel;

namespace Core.Process.Function
{
    abstract class Activator
    {
        protected Activator() { }

        public abstract void Forward(Gpu gpu, BufferField temporaryOutput, ref BufferField output);
        public abstract void Back(Gpu gpu, BufferField sigma, BufferField temporaryOutput, ref BufferField temporarySigma);
    }
}
