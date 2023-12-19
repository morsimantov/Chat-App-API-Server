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
            string id = value.to;
            // contact (who sent the message)
            string contactid = value.from; 

            // extracting the user and the contact from the database
            User user = _context.Users.Where(u => u.id == id).FirstOrDefault();
            if (user == null)
            {

                return NotFound();
            }
            Contact contact = _context.Contacts.Where(c => c.username == id && c.contactid == contactid).FirstOrDefault();
            if (contact == null)
            {
                return NotFound();
            }

      

            // if the contact and user have the same server
            if (contact.server == user.server)
            {
                contact.lastdate = DateTime.Now;
                contact.last = value.content;
            }

            // if the contact and user don't have the same server
            else
            {

                int newChatId;

                // if they don't have a chat together
                if (_context.Chat.Where(c => c.userid == id && c.contactid == contactid).FirstOrDefault() == null)
                {
                    if (_context.Chat.Count() == 0)
                    {
                        newChatId = 1;
                    }
                    else
                    {
                        newChatId = _context.Chat.Max(c => c.id) + 1;
                    }
                    int updatedChatId = newChatId;
                    Chat chat = new Chat() { id = updatedChatId, contactid = contactid, userid = id };
                    _context.Chat.Add(chat);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    newChatId = _context.Chat.Where(c => c.userid == id && c.contactid == contactid).FirstOrDefault().id;
                }

                int followingId;

                if (_context.Messages.Count() != 0)
                {
                    followingId = _context.Messages.Max(e => e.id) + 1;
                }
                else
                {
                    followingId = 1;
                }
                DateTime msgDate = DateTime.Now;
                Message newMessage = new Message() { id = followingId, content = value.content, sent = false, created = msgDate, ChatId = newChatId };
                contact.last = newMessage.content;
                contact.lastdate = newMessage.created;
                _context.Messages.Add(newMessage);
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
            }
         
            return StatusCode(201);
        }
    }
}
