namespace WebApplicationChat.Services
{
    public interface IUserService
    {
        public Task<User> GetUser(string username);
        public Task<bool> AddUser(string username, string nickname, string password, string server);
    }
}
