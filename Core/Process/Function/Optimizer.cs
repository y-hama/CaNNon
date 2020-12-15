using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Field;

namespace Core.Process.Function
{
    abstract class Optimizer
    {
        protected KernelField Kernel { get; private set; }
        public void Initialize(KernelField kernel)
        {
            Kernel = kernel;
            InitializeInnerField();
        }

        protected abstract void InitializeInnerField();


        public void UpdateProcess(int batch)
        {
            UpdateInitial();
            for (int d = 0; d < Kernel.Depth; d++)
            {
                UpdateBias(Kernel.dBias[d] / batch, ref Kernel.Bias[d], d);
                for (int c = 0; c < Kernel.Channels; c++)
                {
                    for (int s = 0; s < Kernel.Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Kernel.Size * 2 + 1; t++)
                        {
                            UpdateKernel(Kernel.dBuffer[c][d][s, t] / batch, ref Kernel.Buffer[c][d][s, t], c, d, s, t);
                        }
                    }
                }
            }
        }

        protected abstract void UpdateInitial();
        protected abstract void UpdateBias(double x, ref double y, int c);
        protected abstract void UpdateKernel(double x, ref double y, int c, int d, int s, int t);
    }
}
