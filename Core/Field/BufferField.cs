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
    class BufferField
    {
        public double[][,] Buffer { get; private set; }
        private double[][] Vector { get; set; }
        private Mat Frame { get; set; } = new Mat();

        public int Channels { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Length { get { return Channels * Width * Height; } }
        public int Area { get { return Width * Height; } }

        public double Sum
        {
            get
            {
                double sum = 0;
                for (int c = 0; c < Channels; c++)
                {
                    sum += Buffer[c].Cast<double>().Sum();
                }
                return sum;
            }
        }

        public double AbsSum
        {
            get
            {
                double sum = 0;
                for (int c = 0; c < Channels; c++)
                {
                    sum += Buffer[c].Cast<double>().Sum(x => Math.Abs(x));
                }
                return sum;
            }
        }

        public double SumRatio
        {
            get
            {
                double sum = 0;
                for (int c = 0; c < Channels; c++)
                {
                    sum += Buffer[c].Cast<double>().Sum(x => Math.Abs(x));
                }
                return sum / Length;
            }
        }

        public double Loss(BufferField target)
        {
            int area = Area;
            int width = Width;
            int height = Height;
            var sb = Buffer;
            var tb = target.Buffer;
            var diff = new double[Length];
            if (GPU != null)
            {
                GPU.For(0, Length, n =>
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    diff[n] = DeviceFunction.Abs(sb[c][x, y] - tb[c][x, y]);
                });
            }
            return diff.Sum() / Length;
        }

        private Gpu GPU { get; set; }

        public BufferField(Gpu gpu, Size size, int c)
        {
            Initialize(gpu, size.Width, size.Height, c);
        }

        public BufferField(Gpu gpu, int w, int h, int c)
        {
            Initialize(gpu, w, h, c);
        }

        private void Initialize(Gpu gpu, int w, int h, int c)
        {
            GPU = gpu;
            Width = w;
            Height = h;
            Channels = c;

            Buffer = new double[c][,];
            Vector = new double[c][];
            for (int i = 0; i < c; i++)
            {
                Buffer[i] = new double[w, h];
                Vector[i] = new double[Area];
            }
        }

        public void ReadFrom(Mat frame)
        {
            Frame = frame;
            Read();
        }

        public void ReadFrom(VideoCapture cap)
        {
            Frame = new Mat();
            cap.Read(Frame);
            Frame = Frame.Flip(FlipMode.Y);
            Read();
        }

        public void ReadFrom(string filename)
        {
            Frame = new Mat(filename);
            Read();
        }

        private void Read()
        {
            var cnl = Frame.Channels();
            if (cnl != Channels)
            {
                if (cnl == 1)
                {
                    Cv2.CvtColor(Frame, Frame, ColorConversionCodes.GRAY2BGR);
                }
                else if (cnl == 3)
                {
                    Cv2.CvtColor(Frame, Frame, ColorConversionCodes.BGR2GRAY);
                }
            }

            var size = new Size(Width, Height);
            if (Frame.Size() != size)
            {
                Frame = Frame.Resize(size);
            }
            if (Frame.Type() != MatType.MakeType(MatType.CV_64F, Channels))
            {
                Frame.ConvertTo(Frame, MatType.MakeType(MatType.CV_64F, Channels), 1.0 / byte.MaxValue);
            }

            Mat[] frames = Frame.Split();
            for (int c = 0; c < Channels; c++)
            {
                Marshal.Copy(frames[c].Data, Vector[c], 0, Area);
            }

            FrameToBuffer();
        }

        public void Show(string title = "title")
        {
            BufferToFrame();

            Mat[] frames = new Mat[Channels];
            for (int c = 0; c < Channels; c++)
            {
                frames[c] = new Mat(new Size(Width, Height), MatType.CV_64FC1);
                Marshal.Copy(Vector[c], 0, frames[c].Data, Area);
                frames[c].ConvertTo(frames[c], MatType.CV_8UC1, byte.MaxValue);
            }
            var frame = new Mat();
            Cv2.Merge(frames, frame);
            Cv2.ImShow(title, frame);
            Cv2.WaitKey(1);
        }

        public void DifferenceOf(BufferField inf, ref BufferField otf)
        {
            int area = Area;
            int width = Width;
            int height = Height;
            var sb = Buffer;
            var rb = inf.Buffer;
            var ob = otf.Buffer;

            if (GPU != null)
            {
                GPU.For(0, Length, n =>
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    ob[c][x, y] = sb[c][x, y] - rb[c][x, y];
                });
            }
            else
            {
                for (int n = 0; n < Length; n++)
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    ob[c][x, y] = sb[c][x, y] - rb[c][x, y];
                }
            }
        }

        [GpuManaged()]
        private void FrameToBuffer()
        {
            int area = Area;
            int width = Width;
            int height = Height;
            var buffer = Buffer;
            var vector = Vector;
            if (GPU != null)
            {
                GPU.For(0, Length, n =>
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    buffer[c][x, y] = vector[c][i];
                });
            }
            else
            {
                for (int n = 0; n < Length; n++)
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    buffer[c][x, y] = vector[c][i];
                }
            }
        }

        [GpuManaged()]
        private void BufferToFrame()
        {
            int area = Area;
            int width = Width;
            int height = Height;
            var buffer = Buffer;
            var vector = Vector;
            if (GPU != null)
            {
                GPU.For(0, Channels * Area, n =>
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    vector[c][i] = buffer[c][x, y];
                });
            }
            else
            {
                for (int n = 0; n < Length; n++)
                {
                    int c = (int)(n / area);
                    int i = n - c * area;
                    int y = (int)(i / width);
                    int x = i - y * width;
                    vector[c][i] = buffer[c][x, y];
                }
            }
        }
    }
}
