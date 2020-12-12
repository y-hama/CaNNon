﻿using System;
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
        private static double RamdomMin { get; set; } = -1;
        private static double RamdomMax { get; set; } = 1;

        public KernelField(int channels, int depth, int size, Optimizer opt)
        {
            Channels = channels;
            Depth = depth;
            Size = size;
            Optimizer = opt;
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
                Optimizer.UpdateProcess(this, batch);
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