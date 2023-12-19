using WebApplicationChat.Data;
using Microsoft.EntityFrameworkCore;

namespace WebApplicationChat.Services
{
    public class MessageService : IMessageService

    {
        private readonly WebApplicationContext _context;

        public MessageService(WebApplicationContext context)
        {
            _context = context;
        }

        public Task<Contact> AddMessage(string contactid, string username, string name, string server)
        {
            throw new NotImplementedException();
        }

        public Task DeleteMessage(string contactid, string username)
        {
            throw new NotImplementedException();
        }

        public Task<Contact> GetMessage(string username, string contactid)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Message>> GetMessages(string username, string contactid)
        {
            throw new NotImplementedException();
        }

        public Task SetMessage(string contactid, string username, string name, string server)
        {
            throw new NotImplementedException();
        }
    }
}
