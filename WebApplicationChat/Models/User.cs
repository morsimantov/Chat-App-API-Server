using System.ComponentModel.DataAnnotations;

namespace WebApplicationChat
{
    public class User
    {
        [Key]
        public string id { get; set; } // the username

        [Required]
        public string nickname { get; set; } // nickname given in registration

        [DataType(DataType.Password)]
        [Required]
        public string password { get; set; }

        public string server { get; set; } // the server address of the user
    }
}