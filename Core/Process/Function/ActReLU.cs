using Core.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Alea.Parallel;

namespace Core.Process.Function
{
    class ActReLU : Activator
    {
        public double Parameter { get; set; } = 0;

        public override void Forward(Gpu gpu, BufferField temporaryOutput, ref BufferField output)
        {
            var ow = output.Width;
            var oh = output.Height;
            var oc = output.Channels;
            var obuf = output.Buffer;
            var tobuf = temporaryOutput.Buffer;

            var p = Parameter;

            gpu.For(0, output.Length, n =>
            {
                int c = (int)(n / (ow * oh));
                int l = n - c * (ow * oh);
                int y = (int)(l / ow);
                int x = l - y * ow;

                obuf[c][x, y] = (tobuf[c][x, y] >= 0) ? tobuf[c][x, y] : (tobuf[c][x, y] * p);
            });
        }

        public override void Back(Gpu gpu, BufferField sigma, BufferField temporaryOutput, ref BufferField temporarySigma)
        {
            var tsw = temporarySigma.Width;
            var tsh = temporarySigma.Height;
            var tsc = temporarySigma.Channels;
            var tsbuf = temporarySigma.Buffer;

            var sbuf = sigma.Buffer;
            var tobuf = temporaryOutput.Buffer;

            var p = Parameter;

            gpu.For(0, temporarySigma.Length, n =>
            {
                int c = (int)(n / (tsw * tsh));
                int l = n - c * (tsw * tsh);
                int y = (int)(l / tsw);
                int x = l - y * tsw;

                tsbuf[c][x, y] = ((tobuf[c][x, y] >= 0) ? 1 : p) * sbuf[c][x, y];
            });
        }
    }
}
