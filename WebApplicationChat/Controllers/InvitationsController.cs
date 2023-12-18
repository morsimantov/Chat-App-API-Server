using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;
namespace WebApplicationChat.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly WebApplicationContext _context;
        private readonly IHubContext<WebApplicationHub> _hubContext;

        public InvitationsController(WebApplicationContext context, IHubContext<WebApplicationHub> HubContext)
        {
            _context = context;
            _hubContext = HubContext;
        }
        public class bodyInvitation
        {
            public string? from { get; set; }
            public string? to { get; set; }
            public string? server { get; set; }
        }

        // POST api/<InvitationsController>
        [HttpPost]
        public async Task<ActionResult> Index([FromBody] bodyInvitation value)
        {
            var id = value.to;
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            Contact newContact = new Contact() { contactid = value.from, username = value.to, name = value.from, server = value.server };
            _context.Contacts.Add(newContact);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(id).SendAsync("refresh");
            return StatusCode(201);
        }
    }
}
