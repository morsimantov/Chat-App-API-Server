using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;
using WebApplicationChat;

namespace WebApplicationChat.Controllers
{
    [Route("api/contacts")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly WebApplicationContext _context;

        public MessagesController(WebApplicationContext context)
        {
            _context = context;
        }

        public class NewMessageObj
        {
            public string? userName { get; set; }
            public string? content { get; set; }
        }


        [HttpGet("{id}/messages")]
        public async Task<ActionResult<IEnumerable<Message>>> getMessages(string userName, string id)
        {
            Contact currentcontact = _context.Contacts.Where(e => e.contactid == id && e.username == userName).FirstOrDefault();
            if (currentcontact == null)
            {
                return NotFound();
            }
            Chat wantedChat = _context.Chat.Where(c => c.userid == userName && c.contactid == id).FirstOrDefault();

            // If there are no messages in the chat yet
            if (wantedChat == null)
            {
                return Ok(new List<Message>());
            }
            // Fetch the messgages from that chat
            var response = await _context.Messages.Where(e => e.ChatId == wantedChat.id).ToListAsync();
            return response;
        }

        // post create a new message between contact and current user --- id is the contact
        [HttpPost("{id}/messages")]
        public async Task<ActionResult<Message>> PostMessage(string id, [FromBody] NewMessageObj newmsgobj)
        {
            // Fetch current contact by id + username
            var currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == newmsgobj.userName).FirstOrDefault();
            if (currentContact == null)
            {
                return NotFound();
            }
            // Fetch the date
            DateTime msgDate = DateTime.Now;
            int newChatId1, newChatId2;
            int followingId;
            // server same check - if they have the same server
            if (_context.Users.Where(e => e.id == newmsgobj.userName).FirstOrDefault().server == currentContact.server)
            {
                // if the user and contact don't have a chat togther
                if (_context.Chat.Where(c => c.userid == newmsgobj.userName && c.contactid == id).FirstOrDefault() == null)
                {
                    // create new chats objects for the contact and for the user (user-contact, contact-user)
                    if (_context.Chat.Count() == 0)
                    {
                        newChatId1 = 1;
                    }
                    else
                    {
                        newChatId1 = _context.Chat.Max(c => c.id) + 1;
                    }
                    newChatId2 = newChatId1 + 1;
                    Chat chat1 = new Chat() { id = newChatId1, contactid = currentContact.contactid, userid = newmsgobj.userName };
                    Chat chat2 = new Chat() { id = newChatId2, contactid = newmsgobj.userName, userid = currentContact.contactid };
                    _context.Chat.Add(chat1);
                    _context.Chat.Add(chat2);
                    await _context.SaveChangesAsync();
                }
                // if they do have a chat togther, extract the chats objects
                else
                {
                    newChatId1 = _context.Chat.Where(c => c.userid == newmsgobj.userName && c.contactid == id).FirstOrDefault().id;
                    newChatId2 = _context.Chat.Where(c => c.userid == id && c.contactid == newmsgobj.userName).FirstOrDefault().id;

                }
                
                if (_context.Messages.Count() != 0)
                {
                    followingId = _context.Messages.Max(e => e.id) + 1;
                }
                // if there are no messages yet
                else
                {
                    followingId = 1;
                }
                Message newmsg1 = new Message() { id = followingId, content = newmsgobj.content, created = msgDate, sent = true, ChatId = newChatId1 };
                Message newmsg2 = new Message() { id = followingId + 1, content = newmsgobj.content, created = msgDate, sent = false, ChatId = newChatId2 };
                currentContact.last = newmsg1.content;
                currentContact.lastdate = newmsg1.created;
                Contact userNameAsContact = _context.Contacts.Where(c => c.username == currentContact.contactid && c.contactid == newmsgobj.userName).FirstOrDefault();
                userNameAsContact.last = newmsg2.content;
                currentContact.lastdate = newmsg2.created;
                _context.Messages.Add(newmsg1);
                _context.Messages.Add(newmsg2);
                ;
            }
            else
            {
                int newChatId;
                // if the user doesn't have a chat with the contact
                if (_context.Chat.Where(c => c.userid == newmsgobj.userName && c.contactid == id).FirstOrDefault() == null)
                {
                    // add a new chat with the contact
                    if (_context.Chat.Count() == 0)
                    {
                        newChatId = 1;
                    }
                    else
                    {
                        newChatId = _context.Chat.Max(c => c.id) + 1;
                    }
                    int updatedChatId = newChatId;
                    Chat chat = new Chat() { id = updatedChatId, contactid = currentContact.contactid, userid = newmsgobj.userName };
                    _context.Chat.Add(chat);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    newChatId = _context.Chat.Where(c => c.userid == newmsgobj.userName && c.contactid == id).FirstOrDefault().id;
                }
                if (_context.Messages.Count() != 0)
                {
                    followingId = _context.Messages.Max(e => e.id) + 1;
                }
                else
                {
                    followingId = 1;
                }
                Message newmsg = new Message() { id = followingId, content = newmsgobj.content, created = msgDate, sent = true, ChatId = newChatId };
                currentContact.last = newmsg.content;
                currentContact.lastdate = newmsg.created;
                _context.Messages.Add(newmsg);
            }
            await _context.SaveChangesAsync();
            return StatusCode(201);


        }

        // get a specific message details --- id is contact and id2 is message Id
        [HttpGet("{id}/messages/{id2}")]
        public async Task<ActionResult<Message>> GetMessage(string userName, string id, int id2)
        {
            User currentUser = _context.Users.Where(u => u.id == userName).FirstOrDefault();
            Contact currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == userName).FirstOrDefault();
            Chat wantedChat = _context.Chat.Where(c => c.userid == currentUser.id && c.contactid == id).FirstOrDefault();
            return _context.Messages.Where(m => m.ChatId == wantedChat.id && m.id == id2).FirstOrDefault();
        }

        // put update a message --- id is contact and id2 is message Id

        [HttpPut("{id}/Messages/{id2}")]
        public async Task<IActionResult> PutMessage(string id, int id2, [FromBody] NewMessageObj newmsgobj)
        {
            Message message = _context.Messages.Where(m => m.id == id2).FirstOrDefault();
            Chat currentChat = _context.Chat.Where(c => c.id == message.ChatId).FirstOrDefault();
            Contact currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == newmsgobj.userName).FirstOrDefault();
            if (currentChat.contactid != currentContact.contactid) // todo change if we change primary keys of contact
            {
                return BadRequest();
            }
            if (message != null)
            {
                message.content = newmsgobj.content;
                message.created = DateTime.Now;
                await _context.SaveChangesAsync();
                return StatusCode(204);
            }
            else
            {
                return NotFound();
            }
        }

        // delete a message --- id is contact and id2 is message Id
        [HttpDelete("{id}/Messages/{id2}")]
        public async Task<ActionResult> DeleteMessage([FromBody] string userName, string id, int id2)
        {
            User currentUser = _context.Users.Where(u => u.id == userName).FirstOrDefault();
            Contact currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == userName).FirstOrDefault();
            Chat wantedChat = _context.Chat.Where(c => c.userid == currentUser.id && c.contactid == id).FirstOrDefault();
            Message currentMessage = _context.Messages.Where(m => m.ChatId == wantedChat.id && m.id == id2).FirstOrDefault();
            if (currentMessage == null)
            {
                return NotFound();
            }
            _context.Messages.Remove(currentMessage);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
