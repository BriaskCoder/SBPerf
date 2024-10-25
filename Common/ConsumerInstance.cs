using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ConsumerInstance
    {
        public int Id { get; set; }

        public string Subscription { get; set; }
        public bool InUse { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }
    }
}
