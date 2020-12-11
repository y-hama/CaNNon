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
    class ConvForward
    {
        [GpuManaged()]
        public void Process(Gpu gpu, BufferField input, KernelField kernel, int stride, ref BufferField output)
        {
            var iw = input.Width;
            var ih = input.Height;
            var ic = input.Channels;
            var ibuf = input.Buffer;

            var ow = output.Width;
            var oh = output.Height;
            var oc = output.Channels;
            var obuf = output.Buffer;

            var ks = kernel.Size;
            var kbias = kernel.Bias;
            var kbuf = kernel.Buffer;

            gpu.For(0, output.Length, n =>
            {
                int c = (int)(n / (ow * oh));
                int l = n - c * (ow * oh);
                int y = (int)(l / ow);
                int x = l - y * ow;

                double v = kbias[c];
                for (int _c = 0; _c < ic; _c++)
                {
                    for (int s = -ks; s <= ks; s++)
                    {
                        for (int t = -ks; t <= ks; t++)
                        {
                            int i = x + s * stride;
                            int j = y + t * stride;
                            if (i >= 0 && i < iw && j > 0 && j < ih)
                            {
                                v += ibuf[_c][i, j] * kbuf[_c][c][s + ks, t + ks];
                            }
                        }
                    }
                }
                obuf[c][x, y] = v;
            });
        }
    }
}
