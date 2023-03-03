using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI1.Models;

namespace ProjectAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnsolvedProblemsController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public UnsolvedProblemsController(ProjectDbContext context)
        {
            _context = context;
        }

        // GET: api/UnsolvedProblems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnsolvedProblem>>> GetUnsolvedProblems()
        {
            return await _context.UnsolvedProblems.ToListAsync();
        }

        // GET: api/UnsolvedProblems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UnsolvedProblem>> GetUnsolvedProblem(long id)
        {
            var unsolvedProblem = await _context.UnsolvedProblems.FindAsync(id);

            if (unsolvedProblem == null)
            {
                return NotFound();
            }

            return unsolvedProblem;
        }

        // PUT: api/UnsolvedProblems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUnsolvedProblem(long id, UnsolvedProblem unsolvedProblem)
        {
            if (id != unsolvedProblem.Id)
            {
                return BadRequest();
            }

            _context.Entry(unsolvedProblem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UnsolvedProblemExists(id))
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

        // POST: api/UnsolvedProblems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<long>> PostUnsolvedProblem(UnsolvedProblem unsolvedProblem)
        {
            var RandomInt64 = new Random();
            unsolvedProblem.Id = RandomInt64.NextInt64();
            _context.UnsolvedProblems.Add(unsolvedProblem);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (UnsolvedProblemExists(unsolvedProblem.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return unsolvedProblem.Id;
        }

        // DELETE: api/UnsolvedProblems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUnsolvedProblem(long id)
        {
            var unsolvedProblem = await _context.UnsolvedProblems.FindAsync(id);
            if (unsolvedProblem == null)
            {
                return NotFound();
            }

            _context.UnsolvedProblems.Remove(unsolvedProblem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UnsolvedProblemExists(long id)
        {
            return _context.UnsolvedProblems.Any(e => e.Id == id);
        }
    }
}
