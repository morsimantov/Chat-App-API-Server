using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat;
using WebApplicationChat.Data;

namespace WebApplicationChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly WebApplicationContext _context;

        public UsersController(WebApplicationContext context)
        {
            _context = context;
        }

        public class userBody
        {
            public string? id { get; set; }
            public string? nickname { get; set; }
            public string? password { get; set; }
            public string? server { get; set; }

        }

        [HttpGet("getUser")]
        public async Task<IActionResult> getUser(string userName)
        {
            var user = await _context.Users.FindAsync(userName);
            if (user == null)
            {
                return NotFound();
            }
          
            return Ok(user);
           
        }

        [HttpGet]
        public async Task<IActionResult> Login(string userName, string password)
        {
            var user = await _context.Users.FindAsync(userName);
            if (user == null)
            {
                return NotFound();
            }
            if (user.password == password)
            {
                return Ok(user);
            }
            return BadRequest();
        }


        [HttpPost]
        public async Task<IActionResult> Register([FromBody] userBody user)
        {
            var userToCreate = await _context.Users.FindAsync(user.id);
            if (userToCreate == null)
            {
                userToCreate = new User { id = user.id, nickname = user.nickname, password = user.password, server = user.server };
                _context.Users.Add(userToCreate);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return BadRequest();
        }

        //// GET: api/Users/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<User>> GetUser(string id)
        //{
        //    var user = await _context.Users.FindAsync(id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return user;
        //}

        //// PUT: api/Users/5
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUser(string id, User user)
        //{
        //    if (id != user.id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(user).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        //// POST: api/Users
        //// To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<User>> PostUser(User user)
        //{
        //    _context.Users.Add(user);
        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        if (UserExists(user.id))
        //        {
        //            return Conflict();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return CreatedAtAction("GetUser", new { id = user.id }, user);
        //}

        //// DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(string id)
        //{
        //    var user = await _context.Users.FindAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    _context.Users.Remove(user);
        //    await _context.SaveChangesAsync();

        //    return NoContent();
        //}

        //private bool UserExists(string id)
        //{
        //    return _context.Users.Any(e => e.id == id);
        //}
    }
}