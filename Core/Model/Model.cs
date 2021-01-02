using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public BufferField Teacher { get; private set; }

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
                width += Teacher.Width;
                height = Math.Max(height, Teacher.Height);

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
                                    res.Buffer[1][startX[startIndex] + x, y + offsety] = 0.5;
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

        private Reader.Reader Reader { get; set; }

        public Model(Alea.Gpu gpu, Reader.Reader reader)
        {
            GPU = gpu;
            Reader = reader;
        }

        public void AddLayer(Process.Property.Property property)
        {
            Layers.Add(Process.Layer.Layer.Load(property));
        }

        public void Confirm(OpenCvSharp.Size sourceSize)
        {
            Layers[0].property.SetInputSize(sourceSize.Width, sourceSize.Height, Reader.ReadChannels);
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

            Reader.ModelReflection(GPU, new Common.ModelEdgeParameter()
            {
                InputSize = new OpenCvSharp.Size(Layers[0].property.Input.Width, Layers[0].property.Input.Height),
                InputChannels = Layers[0].property.Input.Channels,
                OutputSize = new OpenCvSharp.Size(Layers[Layers.Count - 1].property.Output.Width, Layers[Layers.Count - 1].property.Output.Height),
                OutputChannels = Layers[Layers.Count - 1].property.Output.Channels,
            });
            Reader.Start();
        }

        public void Learn(Field.BufferField input, Field.BufferField teacher)
        {
            InferenceProcess(input);
            LearnProcess(teacher);
        }

        public void Learn(int batchcount)
        {
            for (int b = 0; b < batchcount; b++)
            {
                var buf = Reader.GetBuffer();
                InferenceProcess(buf.Input);
                LearnProcess(buf.Teacher);
                Generation++;
                Epoch = buf.Epoch;
                Teacher = buf.Teacher;
            }
            Batch++;
            Update();
        }

        public void Update()
        {
            Difference = Layers[Layers.Count - 1].Difference;
            foreach (var item in Layers)
            {
                item.Update();
            }
        }

        public void Inference(Field.BufferField input, ref Field.BufferField output)
        {
            InferenceProcess(input);
            Layers[Layers.Count - 1].property.Output.CopyTo(output);
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
