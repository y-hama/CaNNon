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
        public void Process(Gpu gpu, BufferField input, KernelField kernel, int dilation, int expand, ref BufferField output)
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
                            int i = x + s * dilation;
                            int j = y + t * dilation;
                            if (i % expand == 0 && j % expand == 0)
                            {
                                int _i = i / expand;
                                int _j = j / expand;
                                if (_i >= 0 && _i < iw && _j >= 0 && _j < ih)
                                {
                                    v += ibuf[_c][_i, _j] * kbuf[_c][c][s + ks, t + ks];
                                }
                            }
                        }
                    }
                }
                obuf[c][x, y] = v;
            });
        }
    }
}
