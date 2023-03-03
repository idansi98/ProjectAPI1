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


        // GET: api/UnsolvedProblems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<UnsolvedProblem>>> GetUnsolvedProblems(string id)
        {
            List<UnsolvedProblem> unsolvedProblems = await _context.UnsolvedProblems.ToListAsync();
            List<UnsolvedProblem> userUnsolvedProblems = new List<UnsolvedProblem>();
            // filter all unsolved problems so only unsolved problems with the same user id are returned
            foreach (UnsolvedProblem unsolvedProblem in unsolvedProblems)
            {
                int profileID = unsolvedProblem.ProfileId;
                // find the profile in DB
                Models.Profile profile = await _context.Profiles.FindAsync(profileID);
                string userID2 = profile.UserId;
                if (userID2 == id)
                {
                    userUnsolvedProblems.Add(unsolvedProblem);
                }
            }
            return Ok(userUnsolvedProblems);    
        }


        private bool UnsolvedProblemExists(long id)
        {
            return _context.UnsolvedProblems.Any(e => e.Id == id);
        }
    }
}
