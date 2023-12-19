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

        public TransferController(WebApplicationContext context, IHubContext<WebApplicationHub> HubContext, HubService hubService, FirebaseService firebaseService)
        {
            _context = context;
            _hubContext = HubContext;
            _hubService = hubService;
            _firebaseService = firebaseService;
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

            // extracting the user and the contact from the database
            User user = _context.Users.Where(u => u.id == username).FirstOrDefault();
            Contact contact = _context.Contacts.Where(c => c.username == username && c.contactid == contactid).FirstOrDefault();
            if (user == null || contact == null)
            {
                return NotFound();
            }


            // Fetch the date
            DateTime msgDate = DateTime.Now;
            int msgId;
            int chatId;

            Chat chat = _context.Chat.Where(c => c.userid == username && c.contactid == contactid).FirstOrDefault();

            // If the user doesn't have a chat with the contact - add a new chat with the contact
            if (chat == null)
            {
                // Generate a new chat id
                chatId = _context.Chat.Any() ? _context.Chat.Max(c => c.id) + 1 : 1;
                chat = new Chat() { id = chatId, contactid = contact.contactid, userid = username };
                _context.Chat.Add(chat);
                await _context.SaveChangesAsync();
            }

            // If the user has a chat with the ontact
            else
            {
                chatId = chat.id;
            }

            // Generate a new message id
            msgId = _context.Messages.Any() ? _context.Messages.Max(e => e.id) + 1 : 1;

            Message newmsg = new Message() { id = msgId, content = value.content, created = msgDate, sent = true, ChatId = chatId };

            // Update contact last message
            contact.last = newmsg.content;
            contact.lastdate = newmsg.created;
            _context.Messages.Add(newmsg);

            await _context.SaveChangesAsync();

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
