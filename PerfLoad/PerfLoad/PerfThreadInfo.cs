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

        public ILogger logger { get; set; }

    }
}
