using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json.Linq;
using NuGet.Common;

namespace WebApplicationChat.Services
{
    public class FirebaseService
    {

        private static Dictionary<string, string> _users = new Dictionary<string, string>();

        public FirebaseService()
        {
        }

        public void AddUserToken(string username, string token)
        {
            _users[username] = token;
        }

        public string? GetToken(string username)
        {
            return _users.GetValueOrDefault(username);
        }


        public void SendNotification(Contact contact, String messageContent)
        {
            try
            {
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile("private_key.json")
                    });
                }

                var token = GetToken(contact.username);

                var message = new FirebaseAdmin.Messaging.Message()
                {
                    // registration token comes from the client FCM SDKs.
                    Token = token,

                    Notification = new Notification()
                    {
                        Title = "New message from " + contact.contactid,
                        Body = messageContent
                    },
                };


                // Send a message to the device corresponding to the provided registration token.
                string response = FirebaseMessaging.DefaultInstance.SendAsync(message).Result;
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
            }


            catch (Exception e)
            {

            }
        }
    }
}
