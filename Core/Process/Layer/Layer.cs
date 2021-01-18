using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Process.Layer
{
    abstract class Layer
    {
        public Property.Property property;

        public abstract void Forward();
        public abstract void Back();
        public abstract void Update();

        protected int BatchCount { get; set; } = 0;
        protected virtual double DifferenceSum { get; set; }
        public virtual double Difference { get { return DifferenceSum / BatchCount; } }

        public static Layer Load(Property.Property property)
        {
            var layer = (Layer)Activator.CreateInstance(property.Connection);
            layer.property = property;
            return layer;
        }

        public static void Connect(Layer prev, Layer next)
        {
            // TODO:サイズチェック
            next.property.Input = prev.property.Output;
            prev.property.Sigma = next.property.Propagater;
        }

        public void ShowOutput(string title)
        {
            property.Output.Show("output" + title);
        }

        public void Refresh()
        {
            DifferenceSum = 0;
            BatchCount = 0;
        }

    }
}
