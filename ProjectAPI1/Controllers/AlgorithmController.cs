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
using System.Diagnostics;
using System.Collections;
using System.Diagnostics.Metrics;
using System.Threading;

namespace ProjectAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlgorithmController : ControllerBase
    {
        private readonly ProjectDbContext _context;
        private readonly object balanceLock = new object();
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private static List<string> UserIDsRunning = new List<string>();

        public AlgorithmController(ProjectDbContext context, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
        }


        // POST: api/Algorithm
        [HttpPost]
        public async Task<ActionResult> PostAlgorithm(ProblemProfile problemProfile)
        {
            Classes.Profile profile = problemProfile.Profile;
            Classes.Problem problem = problemProfile.Problem;
            // check how many algorithms the user is running
            lock (balanceLock)
            {
                if (UserIDsRunning.Count(id => id == problemProfile.Profile.User.UserId) >= 2)
                {
                    return BadRequest();
                }
                UserIDsRunning.Add(profile.User.UserId);
            }
            // get user profile
            if (profile == null || profile.Algorithm == null)
            {
                UserIDsRunning.Remove(profile.User.UserId);
                return BadRequest();
            }

            // save the problem to the database if it doesn't exist
            Models.Problem problem1 = ClassConvert.ConvertProblem(problem);
            if (problem1.Id == 0 || !_context.Problems.Any(p => p.Id == problem1.Id))
            {
                problem1.Id = _context.Problems.Count() + 1;
                problem1.ExtraPreferences = profile.User.UserId;
                // debug profile as json
                Debug.WriteLine(JsonConvert.SerializeObject(profile));

                _context.Problems.Add(problem1);
                await _context.SaveChangesAsync();
            }
            _ = RunAlgorithm(profile, problem, problem1);
            UserIDsRunning.Remove(profile.User.UserId);
            return Ok();
        }


        // GET: api/Algorithm/running/{id}
        [HttpGet("running/{id}")]
        public ActionResult<int> GetRunningAlgorithms(string id)
        {
            int runningCount = UserIDsRunning.Count(uid => uid == id);

            return Ok(runningCount.ToString() + "/2");
        }

        private async Task RunAlgorithm(Classes.Profile profile, Classes.Problem problem, Models.Problem problem1)
        {
            // debug the problem as json
            Debug.WriteLine(JsonConvert.SerializeObject(problem));
            AlgorithmFactory algorithmFactory = new();
            Algorithm alg = algorithmFactory.GetAlgorithm(profile);
            Debug.WriteLine("Algorithm started");

            Classes.Solution solution = alg.GetSolution(problem);
            Debug.WriteLine("Solution !!!!!!!!!!!!!");

            // Save to database
            Models.Solution solution1 = ClassConvert.ConvertSolution(solution);
            solution1.Id = _context.Solutions.Count() + 1;
            solution1.ProblemId = problem1.Id;
            solution1.ProfileId = profile.Id;
            solution1.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ProjectDbContext>();
                context.Solutions.Add(solution1);
                await context.SaveChangesAsync();
            }
        }








    }
}
