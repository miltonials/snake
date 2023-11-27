using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SnakeGameBackend.Models;

namespace SnakeGameFrontend.Controllers
{
    public class PlayController : Controller
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public static IConfiguration _configuration;
        public PlayController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }
        public async Task<IActionResult> Index(string roomId)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            IEnumerable<Jugador> jugadores = GetRoomPlayers(roomId).Result;
            IEnumerable<PartidaEnEspera> partidas = await ChatController.GetPartidasAsync();
            PartidaEnEspera partida = partidas.Where(p => p.CodigoIdentificador == roomId).FirstOrDefault();

            //string apiUrl = Controllers.ChatController._configuration.GetValue<string>("apiUrl");
            //using var httpClient = new HttpClient();
            //string encodedRoom = Uri.EscapeDataString(roomId);
            //string encodedNickname = Uri.EscapeDataString(ViewBag.Jugador.Nickname);
            //string url = $"{apiUrl}/UnirsePartida?identificadorPartida={roomId}&nickname={encodedNickname}&colorSerpiente=ND";
            //using var response = await httpClient.PostAsync(url, null);


            ViewBag.Partida = partida;
            //conseguir numero de jugadores
            ViewBag.RoomId = roomId;
            ViewBag.Jugadores = jugadores;
            ViewBag.CantidadJugadores = partida.CantidadJugadores;
            return View();
        }

        public static async Task<IEnumerable<Jugador>> GetRoomPlayers(string roomId)
        {
            //public IEnumerable<Jugador> JugadoresEnPartida(string identificadorPartida
            string apiUrl = _configuration.GetValue<string>("apiUrl");
            using var httpClient = new HttpClient();
            string encodedRoomId = Uri.EscapeDataString(roomId);
            string url = $"{apiUrl}/JugadoresEnPartida?identificadorPartida={encodedRoomId}";
            using var response = await httpClient.GetAsync(url);
            string apiResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Jugador>>(apiResponse);
        }
    }
}
