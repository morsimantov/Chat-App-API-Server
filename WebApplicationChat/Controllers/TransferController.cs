using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Hubs;
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
            Contact contact = _context.Contacts.Where(c => c.contactid == contactid).FirstOrDefault();
            if (contact == null)
            {
                return NotFound();
            }

            // if the contact and user have the same server
            if(contact.server == user.server)
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
                
            }


            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential =
                        GoogleCredential.FromFile("private_key.json")
                });
            }
         

            var message = new FirebaseAdmin.Messaging.Message()
            {
                // registration token comes from the client FCM SDKs.
                Token = "cLvAWrPZQWeLS5YLM3MIM5:APA91bHf8mYRZpIGbTSZJCYJXfgX2Mxl0p1h5_fpNibTsyV2omaV6pCzRUNpnzr0hOIVAY20UXTODcBNFi4CyiglObVpqMohJ6b2m0Ze0JzmiWcTwRJrPzttgx3I8T66tWgyaQ9OoCnd",

                Notification = new Notification()
                {
                    Title = "New message from " + contactid,
                    Body = value.content
                },
            };


            // Send a message to the device corresponding to the provided registration token.
            string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
            // Response is a message ID string.
            Console.WriteLine("Successfully sent message: " + response);
        
            await _context.SaveChangesAsync();
            await _hubContext.Clients.Group(id).SendAsync("refresh");
            return StatusCode(201);
        }
    }
}
