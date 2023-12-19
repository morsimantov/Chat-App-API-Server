using Microsoft.AspNetCore.SignalR;
using WebApplicationChat.Services;

namespace WebApplicationChat.Hubs
{
    public class WebApplicationHub : Hub
    {

        private readonly HubService _hubService;
        public WebApplicationHub(HubService hubService)
        {
            _hubService = hubService;
        }
        public void connect(string username)
        {

            //await Groups.AddToGroupAsync(Context.ConnectionId, username);
            _hubService.AddUserConnection(username, Context.ConnectionId);

        }
    }
}
