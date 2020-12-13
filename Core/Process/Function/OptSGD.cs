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

        protected override void InitializeInnerField()
        {
        }

        public override void UpdateBias(double x, ref double y, int c)
        {
            UpdateElement(x, ref y);
        }

        public override void UpdateKernel(double x, ref double y, int c, int d, int s, int t)
        {
            UpdateElement(x, ref y);
        }

        private void UpdateElement(double x, ref double y)
        {
            y = y - Rho * x;
        }
    }
}
