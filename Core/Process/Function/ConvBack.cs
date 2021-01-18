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
        
        public void Process(Gpu gpu, BufferField input, BufferField sigma, int dilation, int expand, ref BufferField propagater, ref KernelField kernel)
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

            var dkbias = kernel.dBias;
            var dkbuf = kernel.dBuffer;

            var pw = propagater.Width;
            var ph = propagater.Height;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            #region Update Kernel
            gpu.For(0, sc, c =>
            {
                double db = dkbias[c];
                double ddb = 0;
                for (int x = 0; x < sw; x++)
                {
                    for (int y = 0; y < sh; y++)
                    {
                        ddb += sbuf[c][x, y];
                    }
                }
                dkbias[c] = db + (ddb / (sw * sh));
            });

            gpu.For(0, (2 * ks + 1) * (2 * ks + 1), n =>
            {
                int s = (int)(n / (2 * ks + 1));
                int t = n - s * (2 * ks + 1);
                int _s = s - ks;
                int _t = t - ks;

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
                                int _x = x + _s * dilation;
                                int _y = y + _t * dilation;
                                if (_x % expand == 0 && _y % expand == 0)
                                {
                                    int __x = _x / expand;
                                    int __y = _y / expand;
                                    if (__x >= 0 && __x < iw && __y >= 0 && __y < ih)
                                    {
                                        cnk++;
                                        ddk += ibuf[c][__x, __y] * sbuf[d][x, y];
                                    }
                                }
                            }
                        }
                        dkbuf[c][d][s, t] = dk + ((ddk / (sw * sh * (2 * (ks - 1) + 1))));
                    }
                }
            });
            #endregion

            #region Calculate Propagater
            gpu.For(0, propagater.Length, n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;

                double v = 0;
                for (int _c = 0; _c < sc; _c++)
                {
                    for (int s = ks; s >= -ks; s--)
                    {
                        for (int t = ks; t >= -ks; t--)
                        {
                            int i = x + s * dilation;
                            int j = y + t * dilation;
                            if (i % expand == 0 && j % expand == 0)
                            {
                                int _i = i / expand;
                                int _j = j / expand;
                                if (_i >= 0 && _i < sw && _j >= 0 && _j < sh)
                                {
                                    v += sbuf[_c][_i, _j] * kbuf[c][_c][s + ks, t + ks];
                                }
                            }
                        }
                    }
                }
                pbuf[c][x, y] = v;
            });
            #endregion
        }
    }
}
