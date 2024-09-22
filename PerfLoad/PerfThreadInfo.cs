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
        public int NumberMessages { get; set; } = 10;
        public int NumberConcurrentCalls { get; set; } = 10;

        /// <summary>
        /// Minimum duration in seconds
        /// </summary>
        public int MinimumDuration { get; set; }
        public MsgSize Size { get; set; }
        public decimal Rate { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime FinishTime { get; set; } = DateTime.Now;
        public long Elapsed { get; set; }
        public ILogger logger { get; set; }
        public string ASB_ConnectionString { get; set; }
        public string DB_ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TopicName { get; set; }

        public decimal ActualRate { get; set; }
        public int ActualNumberMessages { get; set; } = 10;
    }
}
