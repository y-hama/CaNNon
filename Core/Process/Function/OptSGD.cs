using Core.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Process.Function
{
    class OptSGD : Optimizer
    {
        public double Rho { get; private set; }

        public OptSGD(double rho)
        {
            Rho = rho;
        }

        public override void UpdateProcess(KernelField kernel, int batch)
        {
            for (int d = 0; d < kernel.Depth; d++)
            {
                for (int c = 0; c < kernel.Channels; c++)
                {
                    kernel.Bias[d] -= Rho * kernel.dBias[d] / batch;
                    for (int s = 0; s < kernel.Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < kernel.Size * 2 + 1; t++)
                        {
                            kernel.Buffer[c][d][s, t] -= Rho * kernel.dBuffer[c][d][s, t] / batch;
                        }
                    }
                }
            }
        }
    }
}
