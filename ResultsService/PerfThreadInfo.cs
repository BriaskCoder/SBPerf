﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResultsService
{
    public class PerfThreadInfo
    {
        public int Id { get; set; }
        public int RunId { get; set; }
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
        public decimal Elapsed { get; set; }

        [NotMapped]
        public ILogger logger { get; set; }
        [NotMapped]
        public string ASB_ConnectionString { get; set; }
        [NotMapped]
        public string DB_ConnectionString { get; set; }
        public string QueueName { get; set; }
        public string TopicName { get; set; }
        public int NumCreated { get; set; }
        public decimal ActualRate { get; set; }
        public int ActualNumberMessages { get; set; } = 10;
    }
}
