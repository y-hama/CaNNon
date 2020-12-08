using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common
{
    class Stopwatch
    {
        private DateTime temporary { get; set; } = DateTime.Now;

        public void Start()
        {
            temporary = DateTime.Now;
        }
        public double Elapsed()
        {
            return Math.Round((DateTime.Now - temporary).TotalMilliseconds, 3);
        }
    }
}
