using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.EntityFrameworkCore;

namespace ResultsService
{
    public class ResultsServiceContext : DbContext
    {
        public ResultsServiceContext (DbContextOptions<ResultsServiceContext> options)
            : base(options)
        {
        }

        public DbSet<PerfThreadInfo> PerfThreadInfo { get; set; } = default!;
        public DbSet<Run> Runs { get; set; } = default!;
    }
}
