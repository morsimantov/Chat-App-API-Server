using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat;
using WebApplicationChat.Data;
using WebApplicationChat.Services;

namespace WebApplicationChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        public class userBody
        {
            public string? id { get; set; }
            public string? nickname { get; set; }
            public string? password { get; set; }
            public string? server { get; set; }

        }

        [HttpGet("getUser")]
        public async Task<IActionResult> getUser(string username)
        {
            var user = await _userService.GetUser(username);
            if (user == null)
            {
                return NotFound();
            }
          
            return Ok(user);
           
        }

        [HttpGet]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _userService.GetUser(username);
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
            var result = await _userService.AddUser(user.id, user.nickname, user.password, user.server);
            if (!result)
            {
                BadRequest();
            }
            return Ok();
        }
    }
}