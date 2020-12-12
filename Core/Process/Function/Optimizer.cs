using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Field;

namespace Core.Process.Function
{
    abstract class Optimizer
    {

        public abstract void UpdateProcess(KernelField kernel, int batch);
    }
}
