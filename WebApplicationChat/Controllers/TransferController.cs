using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;

namespace WebApplicationChat.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private readonly WebApplicationContext _context;
        private readonly IHubContext<WebApplicationHub> _hubContext;

        public TransferController(WebApplicationContext context, IHubContext<WebApplicationHub> HubContext)
        {
            _context = context;
            _hubContext = HubContext;
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
            string id = value.to;   // user id
            string contactid = value.from; // contact
            User user = _context.Users.Where(u => u.id == id).FirstOrDefault();
            if (user == null)
            {

                return NotFound();
            }
            Contact contact = _context.Contacts.Where(c => c.contactid == contactid).FirstOrDefault();
            if (contact == null)
            {
                return NotFound();
            }
            if(contact.server == user.server)
            {
                contact.lastdate = DateTime.Now;
                contact.last = value.content;
            }
            else
            {
                int newChatId;
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
                    newChatId = _context.Chat.Where(c => c.userid == id && c.contactid == id).FirstOrDefault().id;
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
                
            }
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(id).SendAsync("refresh");
            return StatusCode(201);
        }
    }
}
