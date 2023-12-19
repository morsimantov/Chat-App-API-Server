using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using WebApplicationChat.Models;
using WebApplicationChat.Services;

namespace WebApplicationChat.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ConnectFirebaseController : Controller
    {
        private readonly FirebaseService _firebaseService;
        private readonly WebApplicationContext _context;


        public ConnectFirebaseController(FirebaseService firebaseService, WebApplicationContext context)
        {
            _firebaseService = firebaseService;
            _context = context;

        }

        // POST: ConnectFirebase
        [HttpPost]
        public async Task<IActionResult> ConnectFirebase([Bind("username, token")] UserToken userToken)
        {
            if (userToken == null || userToken.username == null || userToken.token == null)
            {
               
                return NotFound();
            }
            var user = await _context.Users.FindAsync(userToken.username);

            if (user == null)
            {
                return NotFound();
            }
            _firebaseService.AddUserToken(userToken.username, userToken.token);
            return Ok();
        }
    }
}