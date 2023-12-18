using WebApplicationChat.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace WebApplicationChat.Services
{
    public class ContactService : IContactService
    {
        private readonly WebApplicationContext _context;

        public ContactService(WebApplicationContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Contact>> GetContacts(string username)
        {
            return await _context.Contacts.Where(contact => contact.username == username).ToListAsync();
        }

        public async Task<Contact> GetContact(string id, string username)
        {
            var contact = await _context.Contacts.FindAsync(id, username);
            return contact;
        }

        public async Task SetContact(string contactid, string username, string name, string server)
        {
            var contact = await _context.Contacts.FindAsync(contactid, username);
            if (contact != null)
            {
                contact.server = server;
                contact.name = name;
                _context.Entry(contact).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Contact> AddContact(string contactid, string username, string name, string server)
        {
            var contact = await GetContact(contactid, username);
            if (contact == null)
            {
                // make sure the contact is registered as a user
                var user = await _context.Users.FindAsync(contactid);
                if (user != null)
                {
                    if (server == user.server)
                    {
                        contact = new Contact() { contactid = contactid, username = username, name = name, server = server }; //todo last and lastdate?
                        _context.Contacts.Add(contact);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            return contact;
        }

        public async Task DeleteContact(string contactid, string username)
        {
            var contact = await _context.Contacts.FindAsync(contactid, username);
            if (contact != null)
            {
                _context.Contacts.Remove(contact);
                await _context.SaveChangesAsync();
            }
        }



    }
}
