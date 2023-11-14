using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace SnakeGameFrontend.Models
{
    public class SnakeGameHub : Hub
    {
        public async Task SendMessage(string room, string user, string message)
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
        }

        public async Task JoinRoom(string room, string nickName)
        {
            await AddPlayer(room, nickName);
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("JugadoresCompletos");
            await Clients.Group(room).SendAsync("ReceiveMessage", "Server", $"{Context.ConnectionId} has joined the room {room}.");
        }

        public async Task LeaveRoom(string room, string user)
        {
            await RemovePlayer(room, user);
            await Clients.Group(room).SendAsync("JugadoresCompletos");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", "Server", $"{user} has left the room {room}.");
        }

        public async Task Start(string room)
        {
            //TODO: modificar para redireccionar a la pagina donde estará la partida
            //el metodo debe ser post para que no se inicie antes de que todos los jugadores esten listos
            await Clients.Group(room).SendAsync("RedirectTo", "/snakeGame?room=" + room);
        }

        private async Task AddPlayer (string room, string nickname)
        {
            Console.WriteLine($"AddPlayer: {room}, {nickname}\n\n\n\n\n");
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(nickname);
            string url = $"{apiUrl}/UnirsePartida?identificadorPartida={room}&nickname={encodedNickname}&colorSerpiente=ND";
            using var response = await httpClient.PostAsync(url, null);
        }


        private async Task RemovePlayer(string room, string nickname)
        {
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(nickname);

            string url = $"{apiUrl}/AbandonarPartida?identificadorPartida={room}&nickname={encodedNickname}";
            using var response = await httpClient.PostAsync(url, null);
        }
    }
}
