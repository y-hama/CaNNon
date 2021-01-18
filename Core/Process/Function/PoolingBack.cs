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

        public void Process(Gpu gpu, Property.PoolingProperty.PoolType type, BufferField sigma, BufferField map, int reduction, int expansion, ref BufferField propagater)
        {
            switch (type)
            {
                case Property.PoolingProperty.PoolType.Max:
                    Max(gpu, sigma, map, reduction, expansion, ref propagater);
                    break;
                case Property.PoolingProperty.PoolType.Min:
                    Min(gpu, sigma, map, reduction, expansion, ref propagater);
                    break;
                case Property.PoolingProperty.PoolType.Average:
                    Average(gpu, sigma, map, reduction, expansion, ref propagater);
                    break;
                default:
                    break;
            }
        }

        public void Max(Gpu gpu, BufferField sigma, BufferField map, int reduction, int expansion, ref BufferField propagater)
        {
            var sw = sigma.Width;
            var sh = sigma.Height;
            var sc = sigma.Channels;
            var sbuf = sigma.Buffer;

            var mbuf = map.Buffer;

            var pw = propagater.Width / reduction;
            var ph = propagater.Height / reduction;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            gpu.For(0, propagater.Length / (reduction * reduction), n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;


                double pool = double.MinValue;
                bool check = false;
                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        int _x = x * expansion + i;
                        int _y = y * expansion + j;
                        double _sbuf = sbuf[c][_x, _y];
                        if (pool < _sbuf)
                        {
                            check = true;
                            pool = _sbuf;
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
                        if (mbuf[c][_x, _y] > 0)
                        {
                            pbuf[c][_x, _y] = pool;
                        }
                    }
                }
            });
        }

        public void Min(Gpu gpu, BufferField sigma, BufferField map, int reduction, int expansion, ref BufferField propagater)
        {
            var sw = sigma.Width;
            var sh = sigma.Height;
            var sc = sigma.Channels;
            var sbuf = sigma.Buffer;

            var mbuf = map.Buffer;

            var pw = propagater.Width / reduction;
            var ph = propagater.Height / reduction;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            gpu.For(0, propagater.Length / (reduction * reduction), n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;


                double pool = double.MaxValue;
                bool check = false;
                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        int _x = x * expansion + i;
                        int _y = y * expansion + j;
                        double _sbuf = sbuf[c][_x, _y];
                        if (pool < _sbuf)
                        {
                            check = true;
                            pool = _sbuf;
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
                        if (mbuf[c][_x, _y] > 0)
                        {
                            pbuf[c][_x, _y] = pool;
                        }
                    }
                }
            });
        }

        public void Average(Gpu gpu, BufferField sigma, BufferField map, int reduction, int expansion, ref BufferField propagater)
        {
            var sw = sigma.Width;
            var sh = sigma.Height;
            var sc = sigma.Channels;
            var sbuf = sigma.Buffer;

            var mbuf = map.Buffer;

            var pw = propagater.Width / reduction;
            var ph = propagater.Height / reduction;
            var pc = propagater.Channels;
            var pbuf = propagater.Buffer;

            gpu.For(0, propagater.Length / (reduction * reduction), n =>
            {
                int c = (int)(n / (pw * ph));
                int l = n - c * (pw * ph);
                int y = (int)(l / pw);
                int x = l - y * pw;


                double pool = 0, count = 0;
                //bool check = false;
                for (int i = 0; i < expansion; i++)
                {
                    for (int j = 0; j < expansion; j++)
                    {
                        int _x = x * expansion + i;
                        int _y = y * expansion + j;
                        double _sbuf = sbuf[c][_x, _y];
                        count++;
                        pool += _sbuf;
                    }
                }
                if (count == 0) { pool = 0; }
                else { pool /= count; }

                for (int i = 0; i < reduction; i++)
                {
                    for (int j = 0; j < reduction; j++)
                    {
                        int _x = x * reduction + i;
                        int _y = y * reduction + j;
                        if (mbuf[c][_x, _y] > 0)
                        {
                            pbuf[c][_x, _y] = pool;
                        }
                    }
                }
            });
        }
    }
}
