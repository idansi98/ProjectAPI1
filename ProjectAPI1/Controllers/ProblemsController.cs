﻿using System;
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
    public class ProblemsController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public ProblemsController(ProjectDbContext context)
        {
            _context = context;
        }

        // TEMP
        // GET api/Problems
        [HttpGet]
        public async Task<ActionResult<List<Classes.Problem>>> GetProblems()
        {
            List<Models.Problem> problems = await _context.Problems.ToListAsync();
            return Ok(ClassConvert.ConvertProblems(problems));
        }


        // GET: api/Problems/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Classes.Problem>>> GetProblems(String id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            List<Models.Problem> problems = await _context.Problems.ToListAsync();
            List<Models.Problem> userProblems = new List<Models.Problem>();
            // filter all problems so only problems with the same user id are returned
            foreach (Models.Problem problem in problems)
            {
                if (problem.ExtraPreferences == id)
                {
                    userProblems.Add(problem);
                }
            }
            return Ok(ClassConvert.ConvertProblems(userProblems));
        }


    }
}
