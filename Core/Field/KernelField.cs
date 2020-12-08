using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using Alea;
using Alea.Parallel;

using OpenCvSharp;

namespace Core.Field
{
    class KernelField
    {
        public double[] Bias { get; set; }
        public double[][][,] Buffer { get; set; }
        public int Size { get; set; }
        public int Channels { get; set; }
        public int Depth { get; set; }

        private static Random rsrc { get; set; } = new Random();
        private static double RamdomMin { get; set; } = -1;
        private static double RamdomMax { get; set; } = 1;

        public KernelField(int channels, int depth, int size)
        {
            Channels = channels;
            Depth = depth;
            Size = size;
            Bias = new double[depth];
            Buffer = new double[channels][][,];
            for (int c = 0; c < channels; c++)
            {
                Buffer[c] = new double[depth][,];
                for (int d = 0; d < depth; d++)
                {
                    Buffer[c][d] = new double[2 * size + 1, 2 * size + 1];
                }
            }
        }

        public void Randmize()
        {
            double[][][,] buffer;
            RandomBuffer(out buffer);

            for (int d = 0; d < Depth; d++)
            {
                Bias[d] = 0;
                for (int c = 0; c < Channels; c++)
                {
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            Buffer[c][d][s, t] = buffer[c][d][s, t] / ((Size * 2 + 1) * (Size * 2 + 1));
                        }
                    }
                }
            }

            for (int d = 0; d < Depth; d++)
            {
                double sum = 0;
                for (int c = 0; c < Channels; c++)
                {
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            sum += Buffer[c][d][s, t];
                        }
                    }
                }
                Bias[d] = -sum;// / (Channels * Depth);
            }

        }

        public void Clear()
        {
            for (int d = 0; d < Depth; d++)
            {
                Bias[d] = 0;
                for (int c = 0; c < Channels; c++)
                {
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            Buffer[c][d][s, t] = 0;
                        }
                    }
                }
            }
        }

        public void Update(double rho, KernelField dk)
        {
            Update(rho, dk.Bias, dk.Buffer);
        }

        public void Update(double rho, double[] dbias = null, double[][][,] dbuffer = null)
        {
            var _dbias = dbias;
            var _dbuffer = dbuffer;
            if (_dbias == null) { RandomBias(out _dbias); }
            if (_dbuffer == null) { RandomBuffer(out _dbuffer); }

            for (int d = 0; d < Depth; d++)
            {
                for (int c = 0; c < Channels; c++)
                {
                    Bias[d] -= rho * _dbias[d];
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            Buffer[c][d][s, t] -= rho * _dbuffer[c][d][s, t];
                        }
                    }
                }
            }
        }

        private double GetRandomSegment(double min = 0, double max = 1)
        {
            return rsrc.NextDouble() * (max - min) + min;
        }

        private void RandomBias(out double[] b)
        {
            b = new double[Depth];
            for (int d = 0; d < Depth; d++)
            {
                b[d] = GetRandomSegment(RamdomMin, RamdomMax);
            }
        }

        private void RandomBuffer(out double[][][,] b)
        {
            b = new double[Channels][][,];
            for (int c = 0; c < Channels; c++)
            {
                b[c] = new double[Depth][,];
                for (int d = 0; d < Depth; d++)
                {
                    b[c][d] = new double[2 * Size + 1, 2 * Size + 1];
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            b[c][d][s, t] = GetRandomSegment(RamdomMin, RamdomMax);
                        }
                    }
                }
            }
        }
    }
}
