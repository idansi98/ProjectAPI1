using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI1.Classes;
using ProjectAPI1.Models;


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

        // Post: api/Users
        [HttpPost]
        public async Task<ActionResult<String>> CreateUser(User user)
        {
            //User user = new User();
            user.UserId = GenerateUserID();
            user.Number = _context.Users.Count() + 1;
            _context.Users.Add(user);
            // give the user a default profile
            Classes.Profile profile = new Classes.Profile();
            profile.IsDefault = 1;
            profile.IsOutdated = 0;
            profile.Algorithm = "Ordered";
            profile.Name = "Default";
            profile.ExtraSettings = "5 minutes";

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

        //Post: api/Users/5
        [HttpPost("{name}")]
        public async Task<ActionResult> UpdatePassword(string name, string oldPassword, string newPassword)
        {
            var profile = await _context.Profiles.FindAsync(id);
            profile.IsOutdated = 1;
            if (profile == null)
            {
                return NotFound();
            }
            // convert the profile to a class profile
            Classes.Profile profile2 = ClassConvert.ConvertProfile(profile);
            // check if the profile is the default profile
            if (profile2.IsDefault == 1)
            {
                return BadRequest();
            }
            else
            {
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
                return NoContent();
            }
        }

        // GET: api/Users/idan
        [HttpGet("idan")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> CheckUserExists(string name)
        {
            // get list of all users
            List<User> users = await _context.Users.ToListAsync();
            // filter all users so only users with the same user id are returned
            users = users.Where(u => u.UserName == name).ToList();
            if(users.Count > 0)
            {
                return Ok(users[0]);
            }
            return Ok(null);
        }

        // GET: api/Users/ido/5
        [HttpGet("ido/{id}")]
        public async Task<ActionResult<User>> CheckUserExistsId(string userId)
        {
            // get list of all users
            List<User> users = await _context.Users.ToListAsync();
            // filter all users so only users with the same user id are returned
            users = users.Where(u => u.UserId == userId).ToList();
            if (users.Count > 0)
            {
                return Ok(users[0]);
            }
            return Ok(null);
        }
        // Delete: api/Users
        [HttpDelete]
        public async Task<IActionResult> DeleteAllUsers()
        {
            // get list of all users
            List<User> users = await _context.Users.ToListAsync();
            // delete all users
            _context.Users.RemoveRange(users);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Ok();
        }

        static string GenerateUserID(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
