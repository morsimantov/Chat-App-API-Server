using WebApplicationChat.Data;
using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Models;
using Google.Apis.Http;

namespace WebApplicationChat.Services
{
    public class MessageService : IMessageService

    {
        private readonly WebApplicationContext _context;
        private readonly ContactService _contactService;
        private readonly UserService _userService;

        public MessageService(WebApplicationContext context, ContactService contactService, UserService userService)
        {
            _context = context;
            _contactService = contactService;
            _userService = userService;

        }
        public async Task<IEnumerable<Message>> GetMessages(string username, string contactid)
        {
            // Fetch current contact
            Contact currentContact = await _contactService.GetContact(contactid, username);
            if (currentContact == null)
            {
                return null;
            }

            // Fetch chat between contact and user
            Chat wantedChat = _context.Chat.Where(c => c.userid == username && c.contactid == contactid).FirstOrDefault();

            // If there are no messages in the chat yet
            if (wantedChat == null)
            {
                return new List<Message>();
            }
            // Fetch the messgages from that chat
            return await _context.Messages.Where(e => e.ChatId == wantedChat.id).ToListAsync();
        }

        public async Task<Message> GetMessage(string contactid, string username, int messageId)
        {
            User currentUser = await _userService.GetUser(username);
            Contact currentContact = await _contactService.GetContact(contactid, username);
            Chat wantedChat = _context.Chat.Where(c => c.userid == currentUser.id && c.contactid == contactid).FirstOrDefault();
            if (currentUser == null || currentContact == null || wantedChat == null)
            {
                return null;
            }
            var msg = _context.Messages.Where(m => m.ChatId == wantedChat.id && m.id == messageId).FirstOrDefault();
            return msg;
        }

        public async Task<Message> AddMessage(string contactid, string username, string content, bool sent)
        {
            // Fetch current contact by contactid + username
            Contact currentContact = await _contactService.GetContact(contactid, username);
            User currentUser = await _userService.GetUser(username);
            if (currentContact == null || currentUser == null)
            {
                return null;
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
                chat = new Chat() { id = chatId, contactid = currentContact.contactid, userid = username };
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

            Message newMsg = new Message() { id = msgId, content = content, created = msgDate, sent = sent, ChatId = chatId };

            // Update contact last message
            currentContact.last = newMsg.content;
            currentContact.lastdate = newMsg.created;
            _context.Messages.Add(newMsg);

            await _context.SaveChangesAsync();

            return newMsg;
        }

        public async Task<bool> SetMessage(string contactid, int messageId, string username, string content)
        {
            User currentUser = await _userService.GetUser(username);
            Contact currentContact = await _contactService.GetContact(contactid, username);
            Chat wantedChat = _context.Chat.Where(c => c.userid == username && c.contactid == contactid).FirstOrDefault();
            if (currentUser == null || currentContact == null || wantedChat == null)
            {
                return false;
            }
            Message message = await GetMessage(contactid, username, messageId);
            if (wantedChat.contactid != currentContact.contactid || message == null)
            {
                return false;
            }
            message.content = content;
            message.created = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteMessage(string contactid, string username, int messageId)
        {
            User currentUser = await _userService.GetUser(username);
            Contact currentContact = await _contactService.GetContact(contactid, username);
            Chat wantedChat = _context.Chat.Where(c => c.userid == username && c.contactid == contactid).FirstOrDefault();
            if (currentUser == null || currentContact == null || wantedChat == null)
            {
                return false;
            }
            Message message = await GetMessage(contactid, username, messageId);
            if (wantedChat.contactid != currentContact.contactid || message == null)         
            {
                return false;
            }
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
