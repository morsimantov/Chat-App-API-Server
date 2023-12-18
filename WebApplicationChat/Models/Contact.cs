using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WebApplicationChat
{
    public class Contact
    {
        [Key]
        [JsonPropertyName("id")]
        public string contactid { get; set; }

        [Key]
        [JsonIgnore]
        public string username { get; set; } // the username of the contact

        public string name { get; set; } // nickname given to a contact by the user 

        public string server { get; set; }  // the server address

        public string last { get; set; } // the last message that was sent

        public DateTime? lastdate { get; set; } // when last message was sent

    }
}