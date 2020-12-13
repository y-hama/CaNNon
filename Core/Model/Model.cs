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
        private List<Process.Layer.Layer> Layers { get; set; } = new List<Process.Layer.Layer>();

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

        public void AddLayer(Process.Layer.Layer layer)
        {
            Layers.Add(layer);
        }

        public void Confirm()
        {
            for (int i = 0; i < Layers.Count - 1; i++)
            {
                Layers[i + 1].property.Input = Layers[i].property.Output;
                Layers[i].property.Sigma = Layers[i + 1].property.Propagater;
            }
        }

        public void Learn(Field.BufferField input, Field.BufferField teacher)
        {
            InferenceProcess(input);
            LearnProcess(teacher);
        }

        public void Update()
        {
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
