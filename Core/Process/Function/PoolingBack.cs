using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Alea;
using Alea.Parallel;

using Core.Field;

namespace Core.Process.Function
{
    class PoolingBack
    {
        [GpuManaged()]
        public void Process(Gpu gpu)
        {
        }
    }
}
