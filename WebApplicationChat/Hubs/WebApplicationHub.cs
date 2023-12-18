using Microsoft.AspNetCore.SignalR;

namespace WebApplicationChat.Hubs
{
    public class WebApplicationHub : Hub
    {
        public async Task connect(string username)
        {

            await Groups.AddToGroupAsync(Context.ConnectionId, username);

        }
    }
}
