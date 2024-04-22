using Microsoft.AspNetCore.SignalR;

namespace SignalRServer1.Hubs
{
    public class ChatHub : Hub
    {

        public async Task Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            await Clients.All.SendAsync("broadcastMessage", name, message);
        }

        public override async Task OnConnectedAsync()
        {
            //https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs?view=aspnetcore-8.0#handle-events-for-a-connection
            await Clients.Caller.SendAsync("broadcastMessage", "Server", $"Connected to Server {AppDomain.CurrentDomain.BaseDirectory}");
            await base.OnConnectedAsync();
        }
    }
}
