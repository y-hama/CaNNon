using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Core.Field;

namespace Core.Model
{
    class Model
    {
        private Alea.Gpu GPU { get; set; }

        public int Batch { get; private set; } = 0;
        public int Generation { get; private set; } = 0;
        public int Epoch { get; private set; } = 0;

        public double Difference { get; private set; }

        private List<Process.Layer.Layer> Layers { get; set; } = new List<Process.Layer.Layer>();

        public BufferField Input { get { return Layers[0].property.Input; } }
        public BufferField Output { get { return Layers[Layers.Count - 1].property.Output; } }
        public BufferField Sigma { get { return Layers[Layers.Count - 1].property.Sigma; } }
        public BufferField Teacher { get; private set; } = null;

        public BufferField ModelField
        {
            get
            {
                int width = Input.Width, height = Input.Height;
                List<int> startX = new List<int>() { 0 };
                for (int i = 0; i < Layers.Count - 1; i++)
                {
                    startX.Add(width);
                    width += Layers[i].property.Output.Width;
                    height = Math.Max(height, Layers[i].property.Output.Height * Layers[i].property.Output.Channels);
                }
                startX.Add(width);
                width += Layers[Layers.Count - 1].property.Output.Width;
                height = Math.Max(height, Layers[Layers.Count - 1].property.Output.Height);
                startX.Add(width);
                if (null != Teacher)
                {
                    width += Teacher.Width;
                    height = Math.Max(height, Teacher.Height + Sigma.Height);
                }

                OpenCvSharp.Size size = new OpenCvSharp.Size(width, height);
                BufferField res = new BufferField(GPU, size, 3);

                int startIndex = 0;

                // Input
                for (int c = 0; c < Input.Channels; c++)
                {
                    for (int x = 0; x < Input.Width; x++)
                    {
                        for (int y = 0; y < Input.Height; y++)
                        {
                            res.Buffer[c][startX[startIndex] + x, y] = Input.Buffer[c][x, y];
                        }
                    }
                }
                startIndex++;

                // Middle
                for (int i = 0; i < Layers.Count - 1; i++)
                {
                    var layer = Layers[i];
                    int offsety = 0;
                    for (int c = 0; c < layer.property.Output.Channels; c++)
                    {
                        double max = layer.property.Output.Buffer[c].Cast<double>().Max();
                        double min = layer.property.Output.Buffer[c].Cast<double>().Min();
                        double abs = Math.Max(Math.Abs(max), Math.Abs(min));
                        abs = abs == 0 ? 1 : abs;
                        for (int x = 0; x < layer.property.Output.Width; x++)
                        {
                            for (int y = 0; y < layer.property.Output.Height; y++)
                            {
                                double e = layer.property.Output.Buffer[c][x, y];
                                int cidx = 0;
                                double scale = 1;
                                if (e < 0) { scale = -1; cidx = 0; }
                                else { scale = 1; cidx = 2; }
                                e = scale * e;
                                res.Buffer[cidx][startX[startIndex] + x, y + offsety] = e;
                                if (e > 1)
                                {
                                    res.Buffer[1][startX[startIndex] + x, y + offsety] = e / abs;
                                }
                            }
                        }
                        offsety += layer.property.Output.Height;
                    }
                    startIndex++;
                }

                // Output
                for (int c = 0; c < Output.Channels; c++)
                {
                    for (int x = 0; x < Output.Width; x++)
                    {
                        for (int y = 0; y < Output.Height; y++)
                        {
                            res.Buffer[c][startX[startIndex] + x, y] = Output.Buffer[c][x, y];
                        }
                    }
                }
                startIndex++;

                if (null != Teacher)
                {
                    // Teacher
                    for (int c = 0; c < Teacher.Channels; c++)
                    {
                        for (int x = 0; x < Teacher.Width; x++)
                        {
                            for (int y = 0; y < Teacher.Height; y++)
                            {
                                res.Buffer[c][startX[startIndex] + x, y] = Teacher.Buffer[c][x, y];
                            }
                        }
                    }

                    // Sigma
                    if (null != Sigma)
                    {
                        for (int c = 0; c < Sigma.Channels; c++)
                        {
                            for (int x = 0; x < Sigma.Width; x++)
                            {
                                for (int y = 0; y < Sigma.Height; y++)
                                {
                                    res.Buffer[c][startX[startIndex] + x, y + Sigma.Height] = Math.Abs(Sigma.Buffer[c][x, y]);
                                }
                            }
                        }
                    }
                }

                return res;
            }
        }

        public BufferField HiddenOutput(int index) { return Layers[index].property.Output; }

        public BufferField InputField
        {
            get
            {
                return Layers[0].property.Input.Congruence();
            }
        }
        public BufferField OutputField
        {
            get
            {
                return Layers[Layers.Count - 1].property.Output.Congruence();
            }
        }

        private Reader.Reader LearningReader { get; set; } = null;
        public Reader.Reader InferenceReader { get; set; } = null;

        public Model(Alea.Gpu gpu, Reader.Reader reader)
        {
            GPU = gpu;
            LearningReader = reader;
        }

        public void AddLayer(Process.Property.Property property)
        {
            Layers.Add(Process.Layer.Layer.Load(property));
        }

        public void Confirm(OpenCvSharp.Size sourceSize)
        {
            Layers[0].property.SetInputSize(sourceSize.Width, sourceSize.Height, LearningReader.ReadChannels);
            for (int i = 1; i < Layers.Count; i++)
            {
                Layers[i].property.SetInputSize(
                    Layers[i - 1].property.Output.Width,
                    Layers[i - 1].property.Output.Height,
                    Layers[i - 1].property.Output.Channels);
            }

            for (int i = 0; i < Layers.Count - 1; i++)
            {
                Layers[i + 1].property.Input = Layers[i].property.Output;
                Layers[i].property.Sigma = Layers[i + 1].property.Propagater;
            }

            if (LearningReader != null)
            {
                LearningReader.ModelReflection(GPU, new Common.ModelEdgeParameter()
                {
                    InputSize = new OpenCvSharp.Size(Layers[0].property.Input.Width, Layers[0].property.Input.Height),
                    InputChannels = Layers[0].property.Input.Channels,
                    OutputSize = new OpenCvSharp.Size(Layers[Layers.Count - 1].property.Output.Width, Layers[Layers.Count - 1].property.Output.Height),
                    OutputChannels = Layers[Layers.Count - 1].property.Output.Channels,
                });
                LearningReader.Start();
            }

            if (InferenceReader != null)
            {
                InferenceReader.ModelReflection(GPU, new Common.ModelEdgeParameter()
                {
                    InputSize = new OpenCvSharp.Size(Layers[0].property.Input.Width, Layers[0].property.Input.Height),
                    InputChannels = Layers[0].property.Input.Channels,
                    OutputSize = new OpenCvSharp.Size(Layers[Layers.Count - 1].property.Output.Width, Layers[Layers.Count - 1].property.Output.Height),
                    OutputChannels = Layers[Layers.Count - 1].property.Output.Channels,
                });
                InferenceReader.Start();
            }
        }

        public void Learn(Field.BufferField input, Field.BufferField teacher)
        {
            InferenceProcess(input);
            LearnProcess(teacher);
        }

        public void Learn(int batchcount)
        {
            foreach (var layer in Layers)
            {
                layer.Refresh();
            }
            for (int b = 0; b < batchcount; b++)
            {
                var buf = LearningReader.GetBuffer();

                //var r = new Random();
                //double segvl = 0;
                //for (int c = 0; c < buf.Input.Channels; c++)
                //    for (int i = 0; i < buf.Input.Width; i++)
                //        for (int j = 0; j < buf.Input.Height; j++)
                //        {
                //            buf.Input.Buffer[c][i, j] = segvl;
                //        }
                //buf.Input.Buffer[0][r.Next(0, buf.Input.Width / 2), r.Next(0, buf.Input.Height / 2)] = 1 - segvl;
                //buf.Input.Buffer[1][r.Next(0, buf.Input.Width), 0] = 1 - segvl;
                //buf.Input.Buffer[2][0, r.Next(0, buf.Input.Height)] = 1 - segvl;

                //double segvh = 0.5;
                //for (int c = 0; c < buf.Teacher.Channels; c++)
                //    for (int i = 0; i < buf.Teacher.Width; i++)
                //        for (int j = 0; j < buf.Teacher.Height; j++)
                //        {
                //            buf.Teacher.Buffer[c][i, j] = segvh;
                //        }
                //for (int i = 0; i < 10; i++)
                //{
                //    for (int j = 0; j < 10; j++)
                //    {
                //        buf.Teacher.Buffer[2][3 + i, 3 - 1 + j] = 1 - segvh;
                //        buf.Teacher.Buffer[2][3 + i, 3 + 1 + j] = 1 - segvh;
                //        buf.Teacher.Buffer[1][0 + i, 0 + j] = 1 - segvh;
                //        buf.Teacher.Buffer[0][3 - 1 + i, 3 + j] = 1 - segvh;
                //    }
                //}

                InferenceProcess(buf.Input);
                LearnProcess(buf.Teacher);
                Generation++;
                Epoch = buf.Epoch;
                Teacher = buf.Teacher;
            }
            Batch++;
            Update();
        }

        private void Update()
        {
            Difference = Layers[Layers.Count - 1].Difference;
            foreach (var item in Layers)
            {
                item.Update();
            }
        }

        public void Inference()
        {
            Teacher?.Clear();
            Sigma?.Clear();
            var reader = InferenceReader != null ? InferenceReader : LearningReader;
            var buf = reader.GetBuffer();
            InferenceProcess(buf.Input);
        }

        private void InferenceProcess(Field.BufferField input)
        {
            input.CopyTo(Layers[0].property.Input);
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Forward();
            }
        }

        private void LearnProcess(Field.BufferField teacher)
        {
            Layers[Layers.Count - 1].property.Output.DifferenceOf(teacher, ref Layers[Layers.Count - 1].property.Sigma);
            for (int i = Layers.Count - 1; i >= 0; i--)
            {
                Layers[i].Back();
            }
        }
    }
}
