using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;
using WebApplicationChat.Services;
using Microsoft.EntityFrameworkCore.Metadata;

namespace WebApplicationChat.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationsController : ControllerBase
    {
        private readonly WebApplicationContext _context;
        private readonly IHubContext<WebApplicationHub> _hubContext;
        private readonly HubService _hubService;
        private readonly ContactService _contactService;

        public InvitationsController(WebApplicationContext context, IHubContext<WebApplicationHub> HubContext, HubService hubService, ContactService contactService)
        {
            _context = context;
            _hubContext = HubContext;
            _hubService = hubService;
            _contactService = contactService;
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
            await _contactService.AddContact(value.from, value.to, value.from, value.server);
            string? connectionID = _hubService.GetConnectionId(id);
            if (connectionID != null)
            {
                await _hubContext.Clients.Client(connectionID).SendAsync("refresh");
            }
            //await _hubContext.Clients.Group(id).SendAsync("refresh");
            return StatusCode(201);
        }
    }
}
