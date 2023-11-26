using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SnakeGameBackend.Models;

namespace SnakeGameFrontend.Models
{
    public class SnakeGameHub : Hub
    {
        private static Dictionary<string, string> playerColors = new Dictionary<string, string>();
        private static HashSet<string> playersReady = new HashSet<string>();
        public async Task SendMessage(string room, string user, string message)
        {
            await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
        }

        public async Task ShowColors()
        {
            await Clients.All.SendAsync("ShowColors");
        }

        public async Task JoinRoom(string room, string nickName)
        {
            await AddPlayer(room, nickName);
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("JugadoresCompletos");
            await Clients.Group(room).SendAsync("ReceiveMessage", "Server", $"{nickName} ha entrado a la sala.");
        }

        public async Task LeaveRoom(string room, string user)
        {
            await RemovePlayer(room, user);
            await Clients.Group(room).SendAsync("JugadoresCompletos");
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("ReceiveMessage", "Server", $"{user} ha abandonado la sala.");
        }

        public async Task RedirectTo(string room)
        {
            // TODO: Modificar para redireccionar a la página donde estará la partida
            // El método debe ser POST para que no se inicie antes de que todos los jugadores estén listos
            await Clients.Group(room).SendAsync("RedirectTo", "/play?room=" + room);
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


        private async Task RemovePlayer(string room, string nickname)
        {
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(nickname);

            string url = $"{apiUrl}/AbandonarPartida?identificadorPartida={room}&nickname={encodedNickname}";
            using var response = await httpClient.PostAsync(url, null);
        }

        public async Task ColorSelected(string room, string user, string selectedColor)
        {
            if (playerColors.ContainsValue(selectedColor))
            {
                // Notificar al usuario que el color está ocupado
                await Clients.Caller.SendAsync("ColorOccupied", selectedColor);
            }
            else
            {
                // Actualizar el color si el jugador ya había seleccionado uno anteriormente
                if (playerColors.ContainsKey(user))
                {
                    // Liberar el color previamente seleccionado
                    playerColors.Remove(user);
                }

                // Asociar el nuevo color seleccionado con el jugador en el diccionario
                playerColors.Add(user, selectedColor);

                // Notificar a los demás jugadores sobre la selección de color
                await asociarColor(room, user, selectedColor);
                await Clients.Group(room).SendAsync("ColorSelected", user, selectedColor, true);

                // Marcar al jugador como listo
                playersReady.Add(user);

                // Obtener la cantidad total de jugadores en la partida
                int totalPlayers = await GetTotalPlayersInRoom(room);

                // Verificar si todos los jugadores están listos para iniciar la partida
                if (playersReady.Count == totalPlayers)
                {
                    await Clients.Group(room).SendAsync("AllPlayersReady", totalPlayers);
                }
            }
        }

        private async Task asociarColor(string room, string user, string selectedColor)
        {
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string encodedRoom = Uri.EscapeDataString(room);
            string encodedNickname = Uri.EscapeDataString(user);
            string encodedColor = Uri.EscapeDataString(selectedColor);

            string url = $"{apiUrl}/AsociarColorJugadorEnPartida?codigoIdentificador={room}&nickname={encodedNickname}&colorSerpiente={encodedColor}";
            using var response = await httpClient.PostAsync(url, null);
        }

        private async Task<int> GetTotalPlayersInRoom(string room)
        {
            string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");

            using var httpClient = new HttpClient();
            string url = $"{apiUrl}/jugadoresListos?codigoIdentificador={room}";
            using var response = await httpClient.GetAsync(url);
            string apiResponse = await response.Content.ReadAsStringAsync();
            return int.Parse(apiResponse);
        }
    }
}
