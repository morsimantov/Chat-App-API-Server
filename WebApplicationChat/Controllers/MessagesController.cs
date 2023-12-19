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


        // Post a new message between contact and current user (id is the contact)
        [HttpPost("{id}/messages")]
        public async Task<ActionResult<Message>> PostMessage(string id, [FromBody] NewMessageObj newmsgobj)
        {
            // Fetch current contact by contactid (id) + username
            var currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == newmsgobj.userName).FirstOrDefault();
            if (currentContact == null)
            {
                return NotFound();
            }
            // Fetch the date
            DateTime msgDate = DateTime.Now;
            int msgId;
            int chatId;

            Chat chat = _context.Chat.Where(c => c.userid == newmsgobj.userName && c.contactid == id).FirstOrDefault();

            // If the user doesn't have a chat with the contact - add a new chat with the contact
            if (chat == null)
            {
                // Generate a new chat id
                chatId = _context.Chat.Any() ? _context.Chat.Max(c => c.id) + 1 : 1;
                chat = new Chat() { id = chatId, contactid = currentContact.contactid, userid = newmsgobj.userName };
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

            Message newmsg = new Message() { id = msgId, content = newmsgobj.content, created = msgDate, sent = true, ChatId = chatId };

            // Update contact last message
            currentContact.last = newmsg.content;
            currentContact.lastdate = newmsg.created;
            _context.Messages.Add(newmsg);

            await _context.SaveChangesAsync();

            return StatusCode(201);
        }


        // Get a specific message details (id is the contact Id and idMessage is message Id)
        [HttpGet("{id}/messages/{idMessage}")]
        public async Task<ActionResult<Message>> GetMessage(string userName, string id, int idMessage)
        {
            User currentUser = _context.Users.Where(u => u.id == userName).FirstOrDefault();
            Contact currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == userName).FirstOrDefault();
            Chat wantedChat = _context.Chat.Where(c => c.userid == currentUser.id && c.contactid == id).FirstOrDefault();
            return _context.Messages.Where(m => m.ChatId == wantedChat.id && m.id == idMessage).FirstOrDefault();
        }


        // Update a message (id is contact Id and idMessage is message Id)
        [HttpPut("{id}/Messages/{idMessage}")]
        public async Task<IActionResult> PutMessage(string id, int idMessage, [FromBody] NewMessageObj newmsgobj)
        {
            Message message = _context.Messages.Where(m => m.id == idMessage).FirstOrDefault();
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


        // Delete a message (id is contact Id and idMessage is message Id)
        [HttpDelete("{id}/Messages/{idMessage}")]
        public async Task<ActionResult> DeleteMessage([FromBody] string userName, string id, int idMessage)
        {
            User currentUser = _context.Users.Where(u => u.id == userName).FirstOrDefault();
            Contact currentContact = _context.Contacts.Where(e => e.contactid == id && e.username == userName).FirstOrDefault();
            Chat wantedChat = _context.Chat.Where(c => c.userid == currentUser.id && c.contactid == id).FirstOrDefault();
            Message currentMessage = _context.Messages.Where(m => m.ChatId == wantedChat.id && m.id == idMessage).FirstOrDefault();
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
