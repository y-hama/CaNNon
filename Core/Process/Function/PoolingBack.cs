using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Alea.Parallel;

using Core.Field;

namespace Core.Process.Function
{
    class PoolingBack
    {
        [GpuManaged()]
        public void Process(Gpu gpu, BufferField sigma, BufferField map, int reduction, int expansion, ref BufferField propagater)
        {
            var sw = sigma.Width;
            var sh = sigma.Height;
            var sc = sigma.Channels;
            var sbuf = sigma.Buffer;

            var mbuf = map.Buffer;

            var pw = propagater.Width;
            var ph = propagater.Height;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            gpu.For(0, propagater.Length, n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;


                double pool = double.MinValue;
                bool check = false;
                if (mbuf[c][x, y] > 0)
                {
                    int _x = (x / reduction) * expansion;
                    int _y = (y / reduction) * expansion;
                    for (int i = 0; i < expansion; i++)
                    {
                        for (int j = 0; j < expansion; j++)
                        {
                            if (pool <(sbuf[c][_x + i, _y + j]))
                            {
                                check = true;
                                pool = sbuf[c][_x + i, _y + j];
                            }
                        }
                    }
                }
                if (!check) { pool = 0; }
                pbuf[c][x, y] = pool;
            });
        }
    }
}
