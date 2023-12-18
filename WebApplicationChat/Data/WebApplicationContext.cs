using Microsoft.EntityFrameworkCore;
using WebApplicationChat;

namespace WebApplicationChat.Data
{
    public class WebApplicationContext : DbContext
    {
        private const string connectionString = "server=localhost;port=3306;database=WebAppDB;user=root;password=1234";
        //private const string connectionString = "Server=(localdb)\\mssqllocaldb;Database=WhenUpDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, MariaDbServerVersion.AutoDetect(connectionString));
            //optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring the Name property as the primary
            // key of the Items table
            //modelBuilder.Entity<User>().HasKey(e => e.id);
            //modelBuilder.Entity<Message>().HasKey(e => e.id);
            //modelBuilder.Entity<Rating>().HasKey(e => e.Id);
            modelBuilder.Entity<Contact>().HasKey(e => new { e.contactid, e.username });

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        //public DbSet<Rating> Ratings { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Chat> Chat { get; set; }


    }
}