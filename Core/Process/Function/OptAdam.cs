using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Field;

namespace Core.Process.Function
{
    class OptAdam : Optimizer
    {
        const double a = 0.001;
        const double b1 = 0.9;
        const double b2 = 0.999;
        const double ep = 10e-8;

        KernelField M { get; set; }
        KernelField V { get; set; }

        protected override void InitializeInnerField()
        {
            M = Kernel.Congruence();
            V = Kernel.Congruence();
        }

        public override void UpdateBias(double x, ref double y, int c)
        {
            UpdateElement(x, ref y, ref M.Bias[c], ref V.Bias[c]);
        }

        public override void UpdateKernel(double x, ref double y, int c, int d, int s, int t)
        {
            UpdateElement(x, ref y, ref M.Buffer[c][d][s, t], ref V.Buffer[c][d][s, t]);
        }

        private void UpdateElement(double x, ref double y, ref double m, ref double v)
        {
            v = (b1) * v + (1 - b1) * x;
            m = (b2) * m + (1 - b2) * x * x;
            y = y - a * (v) / (Math.Sqrt(m + ep));
        }
    }
}
