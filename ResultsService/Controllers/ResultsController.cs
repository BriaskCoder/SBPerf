using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<ActionResult<int>> GetPerfThreadInfo()
        {
            _context.Runs.Add(new Run() { CreatedAt = DateTime.Now, Name = "Test" });
            await _context.SaveChangesAsync();

            int id = _context.Runs.OrderBy(r => r.Id).LastOrDefault().Id;

            return id;
        }

        // GET: api/Results
        [HttpGet("/newrun")]
        public async Task<ActionResult<int>> GetNewRunId()
        {
            _context.Runs.Add(new Run() { CreatedAt = DateTime.Now, Name = "Test" });
            await _context.SaveChangesAsync();

            int id = _context.Runs.OrderBy(r => r.Id).LastOrDefault().Id;

            return id;
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
        public async Task<ActionResult<PerfThreadInfo>> PostPerfThreadInfo(PerfThreadInfo perfThreadInfo)
        {
            _context.PerfThreadInfo.Add(perfThreadInfo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPerfThreadInfo", new { id = perfThreadInfo.Id }, perfThreadInfo);
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
