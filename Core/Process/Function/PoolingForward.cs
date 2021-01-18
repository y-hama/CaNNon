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
    class PoolingForward
    {

        public void Process(Gpu gpu, Property.PoolingProperty.PoolType type, BufferField input, int reduction, int expansion, ref BufferField map, ref BufferField output)
        {
            switch (type)
            {
                case Property.PoolingProperty.PoolType.Max:
                    Max(gpu, input, reduction, expansion, ref map, ref output);
                    break;
                case Property.PoolingProperty.PoolType.Min:
                    Min(gpu, input, reduction, expansion, ref map, ref output);
                    break;
                case Property.PoolingProperty.PoolType.Average:
                    Average(gpu, input, reduction, expansion, ref map, ref output);
                    break;
                default:
                    break;
            }
        }

        private void Max(Gpu gpu, BufferField input, int reduction, int expansion, ref BufferField map, ref BufferField output)
        {
            var iw = input.Width;
            var ih = input.Height;
            var ic = input.Channels;
            var ibuf = input.Buffer;

            var ow = output.Width / expansion;
            var oh = output.Height / expansion;
            var oc = output.Channels;
            var obuf = output.Buffer;

            var mbuf = map.Buffer;

            gpu.For(0, output.Length / (expansion * expansion), n =>
            {
                int c = (int)(n / (ow * oh));
                int l = n - c * (ow * oh);
                int y = (int)(l / ow);
                int x = l - y * ow;

                double pool = double.MinValue;
                bool check = false;
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        double _ibuf = ibuf[c][_x, _y];
                        if (_ibuf > pool)
                        {
                            check = true;
                            pool = _ibuf;
                        }
                    }
                }
                if (!check) { pool = 0; }
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        double _ibuf = ibuf[c][_x, _y];
                        if (_ibuf == pool)
                        {
                            mbuf[c][_x, _y] = 1;
                        }
                    }
                }

                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        obuf[c][x * expansion + i, y * expansion + j] = pool;
                    }
                }
            });
        }

        private void Min(Gpu gpu, BufferField input, int reduction, int expansion, ref BufferField map, ref BufferField output)
        {
            var iw = input.Width;
            var ih = input.Height;
            var ic = input.Channels;
            var ibuf = input.Buffer;

            var ow = output.Width / expansion;
            var oh = output.Height / expansion;
            var oc = output.Channels;
            var obuf = output.Buffer;

            var mbuf = map.Buffer;

            gpu.For(0, output.Length / (expansion * expansion), n =>
            {
                int c = (int)(n / (ow * oh));
                int l = n - c * (ow * oh);
                int y = (int)(l / ow);
                int x = l - y * ow;

                double pool = double.MaxValue;
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        double _ibuf = ibuf[c][_x, _y];
                        if (_ibuf < pool)
                        {
                            pool = _ibuf;
                        }
                    }
                }
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        double _ibuf = ibuf[c][_x, _y];
                        if (_ibuf == pool)
                        {
                            mbuf[c][_x, _y] = 1;
                        }
                    }
                }

                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        obuf[c][x * expansion + i, y * expansion + j] = pool;
                    }
                }
            });
        }

        private void Average(Gpu gpu, BufferField input, int reduction, int expansion, ref BufferField map, ref BufferField output)
        {
            var iw = input.Width;
            var ih = input.Height;
            var ic = input.Channels;
            var ibuf = input.Buffer;

            var ow = output.Width / expansion;
            var oh = output.Height / expansion;
            var oc = output.Channels;
            var obuf = output.Buffer;

            var mbuf = map.Buffer;

            gpu.For(0, output.Length / (expansion * expansion), n =>
            {
                int c = (int)(n / (ow * oh));
                int l = n - c * (ow * oh);
                int y = (int)(l / ow);
                int x = l - y * ow;

                double pool = 0;
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        pool += ibuf[c][_x, _y];
                    }
                }
                pool /= (reduction * reduction);
                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        double _ibuf = ibuf[c][_x, _y];
                        mbuf[c][_x, _y] = 1;
                    }
                }

                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        obuf[c][x * expansion + i, y * expansion + j] = pool;
                    }
                }
            });
        }

    }
}
