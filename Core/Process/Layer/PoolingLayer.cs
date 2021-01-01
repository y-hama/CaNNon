using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Process.Layer
{
    class PoolingLayer : Layer
    {
        private Function.PoolingForward forward => new Function.PoolingForward();
        private Function.PoolingBack back => new Function.PoolingBack();

        private Property.PoolingProperty Property => (Property.PoolingProperty)property;

        public override void Forward()
        {
            forward.Process(Property.GPU,
                Property.Input,
                Property.Reduction, Property.Expansion,
                ref Property.Map, ref Property.Output
                );
        }

        public override void Back()
        {
            back.Process(Property.GPU);
        }

        public override void Update()
        {
            Property.Map.Clear();
        }
    }
}
