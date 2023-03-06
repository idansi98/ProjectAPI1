using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectAPI1.Models;
using ProjectAPI1.Classes;

namespace ProjectAPI1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly ProjectDbContext _context;

        public ProfilesController(ProjectDbContext context)
        {
            _context = context;
        }


        // GET: api/Profiles/5
        [HttpGet("{id}")]
        public async Task<ActionResult<List<Classes.Profile>>> GetProfiles(string id)
        {
            List<Models.Profile> profiles = await _context.Profiles.ToListAsync();
            // filter all profiles so only profiles with the same user id are returned
            profiles = profiles.Where(p => p.UserId == id).ToList();

           List<Classes.Profile> profiles2 = ClassConvert.ConvertProfiles(profiles);
           // filter all the profiles so only profiles that arent outdated are returned
           // profiles2 = profiles2.Where(p => p.IsOutdated == 0).ToList();


            return Ok(profiles2);
        }

        /*// GET: api/Profiles
        [HttpGet]
        public async Task<ActionResult<List<Classes.Profile>>> GetProfiles()
        {
            List<Models.Profile> profiles = await _context.Profiles.ToListAsync();
            // filter all profiles so only profiles with the same user id are returned
            //profiles = profiles.Where(p => p.UserId == id).ToList();
            List<Classes.Profile> profiles2 = ClassConvert.ConvertProfiles(profiles);
            // filter all the profiles so only profiles that arent outdated are returned
            profiles2 = profiles2.Where(p => p.IsOutdated == 0).ToList();
            return Ok(profiles2);
        }*/


        // POST: api/Profiles/5
        [HttpPost ("{id}")]
        public async Task<ActionResult<int>> PostProfile(String id, Classes.Profile profile)
        {
            profile.IsDefault = 0;
            profile.IsOutdated = 0;
            Models.Profile profile1 = ClassConvert.ConvertProfile(profile, id);
            profile1.Id = _context.Profiles.Count() + 1;
            profile1.UserId = id;
            _context.Profiles.Add(profile1);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProfileExists(profile.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            return Ok(profile1.Id);
        }

        // DELETE: api/Profiles/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfile(int id)
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
            } else
            {
                _context.Profiles.Update(profile);
                await _context.SaveChangesAsync();
                return NoContent();
            }


        }
        // profile exists method
        private bool ProfileExists(int id)
        {
            return _context.Profiles.Any(e => e.Id == id);
        }
    }
}
