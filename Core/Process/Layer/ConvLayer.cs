using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Process.Layer
{
    class ConvLayer : Layer
    {
        private Function.ConvForward forward => new Function.ConvForward();
        private Function.ConvBack back => new Function.ConvBack();

        private Property.ConvProperty Property => (Property.ConvProperty)property;

        public override void Forward()
        {
            forward.Process(Property.GPU,
                Property.Input, Property.Kernel,
                Property.Dilation, Property.Expand,
                ref Property.Output
                );
        }

        public override void Back()
        {
            back.Process(Property.GPU,
                Property.Input, Property.Sigma,
                Property.Dilation, Property.Expand,
                ref property.Propagater, ref Property.Kernel);
            DifferenceSum += Property.Sigma.AbsSum / Property.Sigma.Length;
            BatchCount++;
        }

        public override void Update()
        {
            Property.Kernel.Update(BatchCount);
            Property.Kernel.dClear();
            DifferenceSum = 0;
            BatchCount = 0;
        }
    }
}
