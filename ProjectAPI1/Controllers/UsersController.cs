﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI1.Models;

//idan
namespace ProjectAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public UsersController(ProjectDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<String>> CreateUser()
        {
         User user = new User();
            user.UserId = GenerateUserID();
            user.Number = _context.Users.Count() + 1;
            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            // give the user a default profile
            Classes.Profile profile = new Classes.Profile();
            profile.IsDefault = 1;
            profile.IsOutdated = 0;
            profile.Algorithm = "Ordered";
            profile.Name = "Default";
           
            // convert it to a model
            Models.Profile profile1 = ClassConvert.ConvertProfile(profile, user.UserId);
            profile1.UserId = user.UserId;
            // save it to the database
            profile1.Id = _context.Profiles.Count() + 1;
            _context.Profiles.Add(profile1);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Ok(user.UserId); 
        }


        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> CheckUserExists(string id)
        {
            // get list of all users
            List<User> users = await _context.Users.ToListAsync();
            // filter all users so only users with the same user id are returned
            users = users.Where(u => u.UserId == id).ToList();
            if(users.Count > 0)
            {
                return Ok("User exists");
            }
            return NotFound("User doesn't exist");
        }

 

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Number == id);
        }

        static string GenerateUserID(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
