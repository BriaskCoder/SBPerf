using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService
{
    internal class PerfThreadInfo
    {
        public int Id { get; set; }
        public int NumberMessages { get; set; }
        public int Size { get; set; }
        public decimal Rate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime FinishTime { get; set; }
        public long Elapsed { get; set; }
        public ILogger logger { get; set; }

    }
}
