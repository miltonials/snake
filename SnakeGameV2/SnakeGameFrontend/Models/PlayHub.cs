using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SnakeGameBackend.Models;

namespace SnakeGameFrontend.Models
{
    public class PlayHub : Hub
    {
        private async Task RemovePlayer(string room, string nickname)
        {
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(nickname);

            string url = $"{apiUrl}/AbandonarPartida?identificadorPartida={room}&nickname={encodedNickname}";
            using var response = await httpClient.PostAsync(url, null);
        }

        //metodos de la partida para ./play
        public async Task PrepararPartida(string room, string nickName)
        {
            await AddPlayer(room, nickName);
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("CargarDatosJugador");
        }

        private async Task AddPlayer(string room, string nickname)
        {
            Console.WriteLine($"AddPlayer: {room}, {nickname}\n\n\n\n\n");
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(nickname);
            string url = $"{apiUrl}/UnirsePartida?identificadorPartida={room}&nickname={encodedNickname}&colorSerpiente=ND";
            using var response = await httpClient.PostAsync(url, null);
        }
    }
}
