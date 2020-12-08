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

        public static Layer Load(Property.Property property)
        {
            var layer = (Layer)Activator.CreateInstance(property.Connection);
            layer.property = property;
            return layer;
        }

        public void ShowOutput()
        {
            property.Output.Show("output");
        }

    }
}
