﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResultsService;

namespace ResultsService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly ResultsServiceContext _context;

        public ResultsController(ResultsServiceContext context)
        {
            _context = context;
        }

        // GET: api/Results
        [HttpGet]
        public async Task<ActionResult<PerfThreadInfo>> GetPerfThreadInfo()
        {
            _context.PerfThreadInfo.Add(new PerfThreadInfo()
            {
                ActualRate = 1,
                Elapsed = 30,
                ActualNumberMessages = 40,
                FinishTime = DateTime.Now,
                MinimumDuration = 30,
                NumberConcurrentCalls = 10,
                NumberMessages = 1000,
                NumCreated = 999,
                Rate = 3.333M,
                Size = MsgSize.KB128,
                RunId = 666,
                QueueName = "test",
                StartTime = DateTime.Now,
                TopicName = "hello"
            });
            await _context.SaveChangesAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var runDeets = _context.PerfThreadInfo.OrderBy(r => r.Id).LastOrDefault();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return runDeets;
        }

        // GET: api/Results
        [Route("newrun")]
        [HttpGet]
        public async Task<ActionResult<Run>> GetNewRunId()
        {
            _context.Runs.Add(new Run() { CreatedAt = DateTime.Now, Name = "Test" });
            await _context.SaveChangesAsync();

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var runDeets = _context.Runs.OrderBy(r => r.Id).LastOrDefault();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return runDeets;
        }

        // GET: api/Results/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PerfThreadInfo>> GetPerfThreadInfo(int id)
        {
            var perfThreadInfo = await _context.PerfThreadInfo.FindAsync(id);

            if (perfThreadInfo == null)
            {
                return NotFound();
            }

            return perfThreadInfo;
        }

        // PUT: api/Results/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPerfThreadInfo(int id, PerfThreadInfo perfThreadInfo)
        {
            if (id != perfThreadInfo.Id)
            {
                return BadRequest();
            }

            _context.Entry(perfThreadInfo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerfThreadInfoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Results
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PerfThreadInfo>> PostPerfThreadInfo([FromBody]string perfThreadInfoDetails)
        {
            try
            {
                var deets = JsonSerializer.Deserialize<PerfThreadInfo>(perfThreadInfoDetails);
                deets.Id = 0;

                _context.PerfThreadInfo.Add(deets);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetPerfThreadInfo", new { id = deets.Id }, deets);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        // DELETE: api/Results/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePerfThreadInfo(int id)
        {
            var perfThreadInfo = await _context.PerfThreadInfo.FindAsync(id);
            if (perfThreadInfo == null)
            {
                return NotFound();
            }

            _context.PerfThreadInfo.Remove(perfThreadInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PerfThreadInfoExists(int id)
        {
            return _context.PerfThreadInfo.Any(e => e.Id == id);
        }
    }
}
