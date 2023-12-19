using WebApplicationChat.Models;

namespace WebApplicationChat.Services
{
    public interface IMessageService
    {
        public Task<IEnumerable<Message>> GetMessages(string username, String contactid);
        public Task<Contact> GetMessage(string username, String contactid);
        public Task SetMessage(string contactid, string username, string name, string server);
        public Task<Contact> AddMessage(string contactid, string username, string name, string server);
        public Task DeleteMessage(string contactid, string username);

    }
}
