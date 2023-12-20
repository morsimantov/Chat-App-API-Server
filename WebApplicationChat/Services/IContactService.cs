namespace WebApplicationChat.Services
{
    public interface IContactService
    {
        public Task<IEnumerable<Contact>> GetContacts(string username);
        public Task<Contact> GetContact(string contactid, string username);
        public Task<Contact> AddContact(string contactid, string username, string name, string server);
        public Task SetContact(string contactid, string username, string name, string server);
        public Task DeleteContact(string contactid, string username);

    }
}
