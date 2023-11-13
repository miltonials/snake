using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SnakeGameBackend.Models;
//using SnakeGameBackend.Models.Partida;
using SnakeGameFrontend.Models;
using System.Diagnostics;

namespace SnakeGameFrontend.Controllers
{
    // solo puede ingresar si inicio sesion
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public ChatController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Rooms()
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            IEnumerable<PartidaEnEspera>? partidas = await GetPartidasAsync();
            return View(partidas);
        }

        public IActionResult Room(string room)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return View("Room", room);
        }

        private async Task<IEnumerable<PartidaEnEspera>?> GetPartidasAsync()
        {
            IEnumerable<PartidaEnEspera>? partidas = null;
            string apiUrl = _configuration.GetValue<string>("apiUrl");

            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.GetAsync($"{apiUrl}/PartidasEnProceso");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    partidas = JsonConvert.DeserializeObject<IEnumerable<PartidaEnEspera>>(apiResponse);
                }
            }
            return partidas;
        }
    }
}