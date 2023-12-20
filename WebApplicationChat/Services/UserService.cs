using Microsoft.EntityFrameworkCore;
using WebApplicationChat.Data;

namespace WebApplicationChat.Services
{
    public class UserService : IUserService

    {
        private readonly WebApplicationContext _context;
        public UserService(WebApplicationContext context)
        {
            _context = context;
        }

        public async Task<User> GetUser(string username)
        {
            return await _context.Users.FindAsync(username);
        }

        public async Task<bool> AddUser(string username, string nickname, string password, string server)
        {
            var userToCreate = await _context.Users.FindAsync(username);
            if (userToCreate != null)
            {
                return false;
            }
            userToCreate = new User { id = username, nickname = nickname, password = password, server = server };
            _context.Users.Add(userToCreate);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
