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
        public IActionResult Index(string roomId)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            IEnumerable<Jugador> jugadores = GetRoomPlayers(roomId).Result;
            //conseguir numero de jugadores
            ViewBag.RoomId = roomId;
            ViewBag.Jugadores = jugadores;
            ViewBag.CantidadJugadores = jugadores.Count();
            return View();
        }

        private async Task<IEnumerable<Jugador>> GetRoomPlayers(string roomId)
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
