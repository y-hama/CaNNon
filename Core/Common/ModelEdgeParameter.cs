using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    class ModelEdgeParameter
    {
        public OpenCvSharp.Size InputSize { get; set; }
        public OpenCvSharp.Size OutputSize { get; set; }
        public int InputChannels { get; set; }
        public int OutputChannels { get; set; }
    }
}
