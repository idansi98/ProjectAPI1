using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI1.Models;
using ProjectAPI1.Classes;
using Newtonsoft.Json;

namespace ProjectAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlgorithmController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public AlgorithmController(ProjectDbContext context)
        {
            _context = context;
        }


        // POST: api/Algorithm
        [HttpPost]
        public async Task<ActionResult> PostAlgorithm(ProblemProfile problemProfile)
        {
            Classes.Profile profile = problemProfile.Profile;
            Classes.Problem problem = problemProfile.Problem;
            // get user profile
            if (profile == null || profile.Algorithm == null)
            {
                return BadRequest();
            }
            // solve problem
            AlgorithmFactory algorithmFactory = new();
            Algorithm alg = algorithmFactory.GetAlgorithm(profile);
            Classes.Solution solution = alg.GetSolution(problem);

            // Save to database
            Models.Solution solution1 = ClassConvert.ConvertSolution(solution);
            _context.Solutions.Add(solution1);

            // send to user
            return Ok(solution);
        }
    }
}
