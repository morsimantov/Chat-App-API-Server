using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;
using WebApplicationChat.Services;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;

namespace WebApplicationChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly WebApplicationContext _context;
        private readonly IHubContext<WebApplicationHub> _hubContext;
        private readonly HubService _hubService;
        private readonly FirebaseService _firebaseService;
        private readonly MessageService _messageService;
        private readonly ContactService _contactService;

        public TransferController(WebApplicationContext context, IHubContext<WebApplicationHub> HubContext, HubService hubService, FirebaseService firebaseService, 
            MessageService messageService, ContactService contactService)
        {
            _context = context;
            _hubContext = HubContext;
            _hubService = hubService;
            _firebaseService = firebaseService;
            _messageService = messageService;
            _contactService = contactService;
        }
        public class bodyTransfer
        {
            public string? from { get; set; }  // contact id
            public string? to { get; set; }   // user id
            public string? content { get; set; }
        }

        // POST api/<TransferController>
        [HttpPost]
        public async Task<ActionResult> Index([FromBody] bodyTransfer value)
        {
            // user id (to whom the message was sent)
            string username = value.to;
            // contact (who sent the message)
            string contactid = value.from;

            // extracting the contact from the database
            Contact contact = await _contactService.GetContact(contactid, username);
            if (contact == null)
            {
                return NotFound();
            }

            var message = await _messageService.AddMessage(contactid, username, value.content, false);

            if (message == null)
            {
                return NotFound();
            }

            // get connectionId of the username that supposed to get the message
            string? connectionID = _hubService.GetConnectionId(value.to);

            if (!string.IsNullOrEmpty(connectionID))
            {
                // use SignalR to push the message to the user
                await _hubContext.Clients.Client(connectionID).SendAsync("refresh");
            }

            // get the token of the user
            connectionID = _firebaseService.GetToken(value.to);

            //await _hubContext.Clients.Group(id).SendAsync("refresh");

            if (!string.IsNullOrEmpty(connectionID) && contact != null)
            {
                try
                {
                    // send notification to the user through Firebase
                    _firebaseService.SendNotification(contact, value.content);
                }
                catch (Exception e)
                {

                }
            }

            return StatusCode(201);
        }
    }
}
