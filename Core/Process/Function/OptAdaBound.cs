using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Field;

namespace Core.Process.Function
{
    class OptAdaBound : Optimizer
    {
        const double a = 0.001;
        const double a_ = 0.05;
        const double b1 = 0.9;
        const double b2 = 0.999;
        const double ep = 10e-8;

        const double eta_l_e = 0.025;
        const double eta_u_e = 0.25;
        const double eta_u_t = 0.1;
        const double eta_u_m = 100;

        int t { get; set; } = 0;
        KernelField M { get; set; }
        KernelField V { get; set; }

        protected override void InitializeInnerField()
        {
            M = Kernel.Congruence();
            V = Kernel.Congruence();
        }

        protected override void UpdateInitial()
        {
            t++;
        }

        protected override void UpdateBias(double x, ref double y, int c)
        {
            UpdateElement(x, ref y, ref M.Bias[c], ref V.Bias[c]);
        }

        protected override void UpdateKernel(double x, ref double y, int c, int d, int s, int t)
        {
            UpdateElement(x, ref y, ref M.Buffer[c][d][s, t], ref V.Buffer[c][d][s, t]);
        }

        private void UpdateElement(double x, ref double y, ref double m, ref double v)
        {
            m = (b1) * m + (1 - b1) * x;
            v = (b2) * v + (1 - b2) * x * x;
            y = y - Clip(a / Math.Sqrt(v), Eta_l(t), Eta_u(t)) * (m);
        }

        private double Clip(double prop, double eta_l, double eta_u)
        {
            if (prop < eta_l)
            {
                prop = eta_l;
            }
            if (prop > eta_u)
            {
                prop = eta_u;
            }
            return prop;
        }

        private double Eta_l(double t)
        {
            double dt = (1.0 / eta_l_e) * Math.PI / 2;
            return a_ * (1.0 / (1 + Math.Exp(-eta_l_e * (t - dt))));
        }
        private double Eta_u(double t)
        {
            return eta_u_m * (1 - (1.0 / (1 + Math.Exp(-eta_u_e * ((eta_u_t * t)))))) + a_;
        }
    }
}
