﻿using System;
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

        public void AddLayer(Process.Layer.Layer layer)
        {
            Layers.Add(layer);
        }

        public void Confirm(OpenCvSharp.Size sourceSize)
        {
            Layers[0].property.SetInputSize(sourceSize.Width, sourceSize.Height);
            for (int i = 1; i < Layers.Count; i++)
            {
                Layers[i].property.SetInputSize(Layers[i - 1].property.Output.Width, Layers[i - 1].property.Output.Height);
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
