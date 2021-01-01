using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using Alea;
using Alea.Parallel;

using OpenCvSharp;
using Core.Process.Function;

namespace Core.Field
{
    class KernelField
    {
        public int Size { get; set; }
        public int Channels { get; set; }
        public int Depth { get; set; }

        public double[] Bias { get; set; }
        public double[][][,] Buffer { get; set; }
        public double[] dBias { get; set; }
        public double[][][,] dBuffer { get; set; }

        public Optimizer Optimizer { get; private set; }

        private static Random rsrc { get; set; } = new Random();
        private double RamdomBiasCenter { get; set; } = 0;
        private double RamdomBiasSigma { get; set; } = 0.01;
        private double RamdomBufferCenter { get; set; } = 0;
        private double RamdomBufferSigma { get; set; } = 0.25;

        public KernelField(int channels, int depth, int size, Optimizer opt)
        {
            Channels = channels;
            Depth = depth;
            Size = size;
            Bias = new double[depth];
            Buffer = new double[channels][][,];
            dBias = new double[depth];
            dBuffer = new double[channels][][,];
            for (int c = 0; c < channels; c++)
            {
                Buffer[c] = new double[depth][,];
                dBuffer[c] = new double[depth][,];
                for (int d = 0; d < depth; d++)
                {
                    Buffer[c][d] = new double[2 * size + 1, 2 * size + 1];
                    dBuffer[c][d] = new double[2 * size + 1, 2 * size + 1];
                }
            }

            if (opt != null)
            {
                Optimizer = opt;
                Optimizer.Initialize(this);
            }
        }

        public KernelField Congruence()
        {
            var k = new KernelField(Channels, Depth, Size, null);
            return k;
        }

        public void Randmize()
        {
            double areasize = ((Size * 2 + 1) * (Size * 2 + 1));
            RamdomBiasSigma = 2.0 / (Depth * areasize);
            RamdomBufferSigma = 2.0 / (Depth * areasize);

            double[] bias;
            double[][][,] buffer;
            RandomBias(out bias);
            RandomBuffer(out buffer);

            for (int d = 0; d < Depth; d++)
            {
                Bias[d] = bias[d];
                for (int c = 0; c < Channels; c++)
                {
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            Buffer[c][d][s, t] = buffer[c][d][s, t];
                        }
                    }
                }
            }
        }

        public void dClear()
        {
            for (int d = 0; d < Depth; d++)
            {
                dBias[d] = 0;
                for (int c = 0; c < Channels; c++)
                {
                    for (int s = 0; s < Size * 2 + 1; s++)
                    {
                        for (int t = 0; t < Size * 2 + 1; t++)
                        {
                            dBuffer[c][d][s, t] = 0;
                        }
                    }
                }
            }
        }

        public void Update(int batch)
        {
            Update(dBias, dBuffer, batch);
        }

        public void Update(double[] dbias = null, double[][][,] dbuffer = null, int batch = 1)
        {
            var _dbias = dbias;
            var _dbuffer = dbuffer;
            if (_dbias == null) { RandomBias(out _dbias); }
            if (_dbuffer == null) { RandomBuffer(out _dbuffer); }

            if (batch >= 1)
            {
                Optimizer.UpdateProcess(batch);
            }
        }

        private double GetRandomSegment(double mu = 0, double sigma = 1)
        {
            double rand = 0.0;
            while ((rand = rsrc.NextDouble()) == 0.0) ;
            double rand2 = rsrc.NextDouble();
            double normrand = Math.Sqrt(-2.0 * Math.Log(rand)) * Math.Cos(2.0 * Math.PI * rand2);
            normrand = normrand * sigma + mu;
            return normrand;
        }

        private void RandomBias(out double[] b)
        {
            b = new double[Depth];
            for (int d = 0; d < Depth; d++)
            {
                b[d] = GetRandomSegment(RamdomBiasCenter, RamdomBiasSigma);
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
                            b[c][d][s, t] = GetRandomSegment(RamdomBufferCenter, RamdomBufferSigma);
                        }
                    }
                }
            }
        }
    }
}
