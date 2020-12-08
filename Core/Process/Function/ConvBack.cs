using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using Alea;
using Alea.Parallel;

using OpenCvSharp;

using Core.Field;

namespace Core.Process.Function
{
    class ConvBack
    {
        [GpuManaged()]
        public void Process(Gpu gpu, BufferField input, BufferField sigma, KernelField kernel, ref BufferField propagater, ref KernelField dkernel)
        {
            var iw = input.Width;
            var ih = input.Height;
            var ic = input.Channels;
            var ibuf = input.Buffer;

            var sw = sigma.Width;
            var sh = sigma.Height;
            var sc = sigma.Channels;
            var sbuf = sigma.Buffer;

            var ks = kernel.Size;
            var kbias = kernel.Bias;
            var kbuf = kernel.Buffer;

            var dks = dkernel.Size;
            var dkbias = dkernel.Bias;
            var dkbuf = dkernel.Buffer;

            var pw = propagater.Width;
            var ph = propagater.Height;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            gpu.For(0, sc, c =>
            {
                dkbias[c] = 0;
                for (int x = 0; x < sw; x++)
                {
                    for (int y = 0; y < sh; y++)
                    {
                        dkbias[c] += sbuf[c][x, y];
                    }
                }
                dkbias[c] /= sw * sh * ic;
            });

            gpu.For(0, (2 * dks + 1) * (2 * dks + 1), n =>
            {
                int s = (int)(n / (2 * dks + 1));
                int t = n - s * (2 * dks + 1);
                int _s = s - dks;
                int _t = t - dks;
                for (int c = 0; c < ic; c++)
                {
                    for (int d = 0; d < sc; d++)
                    {
                        double dk = dkbuf[c][d][s, t];
                        double ddk = 0;
                        int cnk = 0;
                        for (int x = 0; x < sw; x++)
                        {
                            for (int y = 0; y < sh; y++)
                            {
                                int _x = x + _s;
                                int _y = y + _t;
                                if (_x >= 0 && _x < iw && _y >= 0 && _y < ih)
                                {
                                    cnk++;
                                    ddk += ibuf[c][_x, _y] * sbuf[d][x, y];
                                }
                            }
                        }
                        dkbuf[c][d][s, t] = dk + ((ddk / (sw * sh * (2 * (dks - 1) + 1))) / DeviceFunction.Sqrt(cnk));
                    }
                }
            });

            gpu.For(0, propagater.Length, n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;

                double v = 0;
                for (int _c = 0; _c < sc; _c++)
                {
                    for (int s = dks; s >= -dks; s--)
                    {
                        for (int t = dks; t >= -dks; t--)
                        {
                            int i = x + s;
                            int j = y + t;
                            if (i >= 0 && i < sw && j > 0 && j < sh)
                            {
                                v += sbuf[_c][i, j] * kbuf[_c][c][s + dks, t + dks];
                            }
                        }
                    }
                }
                pbuf[c][x, y] = v;
            });
        }
    }
}
