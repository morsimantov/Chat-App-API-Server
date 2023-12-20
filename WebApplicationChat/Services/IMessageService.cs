using System.Net.Http;
using WebApplicationChat.Models;

namespace WebApplicationChat.Services
{
    public interface IMessageService
    {
        public Task<IEnumerable<Message>> GetMessages(string username, String contactid);
        public Task<Message> GetMessage(string contactid, string username, int idMessage);
        public Task<Message> AddMessage(string contactid, string username, string content, bool sent);
        public Task<bool> SetMessage(string contactid, int messageId, string username, string content);
        public Task<bool> DeleteMessage(string contactid, string username, int messageId);
    }
}
