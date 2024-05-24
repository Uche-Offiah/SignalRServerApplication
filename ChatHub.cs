using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;

namespace SignalRChatApplication
{
    public class ChatHub : Hub
    {
        private readonly IConnectionMultiplexer _redisConnection;

        public ChatHub(IConnectionMultiplexer redisConnection)
        {
            _redisConnection = redisConnection;
        }
        public async Task SendMessage(string user, string message)
        {
            // Save message to Redis for persistence
            IDatabase db = _redisConnection.GetDatabase();
            await db.ListLeftPushAsync("chatMessages", $"{user}: {message}");

            //await Clients.All.SendAsync("ReceiveMessage", user, message);
            await Clients.Client(this.Context.ConnectionId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has joined the group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} has left the group {groupName}");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
