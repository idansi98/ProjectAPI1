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


        // GET: api/Solutions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Classes.Solution>>> GetSolutions(string userID)
        {
            List<Models.Solution> solutions = await _context.Solutions.ToListAsync();
            List<Models.Solution> userSolutions = new List<Models.Solution>();
            // filter all solutions so only solutions with the same user id are returned
            foreach (Models.Solution solution in solutions)
            {
                int profileID = solution.ProfileId;
                // find the profile in DB
                Models.Profile profile = await _context.Profiles.FindAsync(profileID);
                string userID2 = profile.UserId;
                if (userID2 == userID)
                {
                    userSolutions.Add(solution);
                }
            }
            return Ok(ClassConvert.ConvertSolutions(userSolutions));
        }


        private bool SolutionExists(int id)
        {
            return _context.Solutions.Any(e => e.Id == id);
        }
    }
}
