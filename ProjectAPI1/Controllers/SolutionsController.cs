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
    public class SolutionsController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public SolutionsController(ProjectDbContext context)
        {
            _context = context;
        }
        //TEMP DO NOT USE SPARINGLY
        // GET: api/Solutions
        [HttpGet]
        public async Task<ActionResult<List<Classes.Solution>>> GetSolutions()
        {
            List<Models.Solution> solutions = await _context.Solutions.ToListAsync();
            return Ok(ClassConvert.ConvertSolutions(solutions));
        }

        // BY PROBLEM ID
        // GET: api/Solutions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Classes.Solution>>> GetSolutions(int id)
        {
            List<Models.Solution> solutions = await _context.Solutions.ToListAsync();
            List<Models.Solution> problemSolutions = new List<Models.Solution>();
            // filter all solutions so only solutions with the same problem are reurned
            foreach (Models.Solution solution in solutions)
            {
                if (solution.ProblemId == id)
                {
                    problemSolutions.Add(solution);
                }
            }
            return Ok(ClassConvert.ConvertSolutions(problemSolutions));
        }


        private bool SolutionExists(int id)
        {
            return _context.Solutions.Any(e => e.Id == id);
        }
    }
}
