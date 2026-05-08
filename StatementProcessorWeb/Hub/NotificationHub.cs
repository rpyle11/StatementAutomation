using Microsoft.AspNetCore.SignalR;

namespace StatementProcessorWeb.Hub
{
    public class NotificationHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ServerMessage", message);
        }
    }
}
