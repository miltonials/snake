using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SnakeGameFrontend.Models
{
    public class SnakeGameHub : Hub
    {
        public async Task SendSnake(string snake)
        {
            await Clients.All.SendAsync("ReceiveSnake", snake);
        }

        public async Task SendMessage(string room, string user, string message)
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinRoom(string room)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", "Server", $"{Context.ConnectionId} has joined the room {room}.");
        }
    }
}
