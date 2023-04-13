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
        [HttpPost("{email}")]
        public async Task<ActionResult> UpdatePassword(string email, string newPassword)
        {
            List<Models.User> users = await _context.Users.ToListAsync();
            User user = null;
            user = users.Find(x => x.Email == email);
            user.Password = newPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/Users/idan
        [HttpGet("idan")]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // GET: api/Users/5
        [HttpGet("{email}")]
        public async Task<ActionResult<User>> CheckUserExists(string email)
        {
            // get list of all users
            List<User> users = await _context.Users.ToListAsync();
            // filter all users so only users with the same user id are returned
            users = users.Where(u => u.Email == email).ToList();
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

        // GET: api/Users/maor/5
        [HttpGet("maor/{email}")]
        public async Task<ActionResult<string>> GenerateTempPassword(string email)
        {
            // get list of all users
            List<Models.User> users = await _context.Users.ToListAsync();
            // filter all users so only users with the same user id are returned
            User user = null;
            user = users.Find(u => u.Email == email);
            string tempPassword = CreateNewPassword();
            user.Password = tempPassword;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return Ok(tempPassword);
        }

        // GET: api/Users/van
        [HttpGet("van")]
        public async Task<ActionResult<string>> GenerateValidationString()
        {
            string validationString = CreateValidationString();
            return Ok(validationString);
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

        static string CreateNewPassword(int length = 10)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        static string CreateValidationString(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}
