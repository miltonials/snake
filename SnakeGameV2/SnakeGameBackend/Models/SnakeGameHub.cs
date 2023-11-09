using Microsoft.AspNetCore.SignalR;

namespace SnakeGameFrontend.Models
{
    public class SnakeGameHub : Hub
    {
        public async Task SendSnake(string snake)
        {
            await Clients.All.SendAsync("ReceiveSnake", snake);
        }
    }
}
