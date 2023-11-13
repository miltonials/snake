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
        //private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public static IConfiguration _configuration;
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

        public async Task<IActionResult> Room(string room)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            IEnumerable<PartidaEnEspera>? partidas = await GetPartidasAsync();
            PartidaEnEspera? partida = partidas.Where(p => p.CodigoIdentificador == room).FirstOrDefault();

            //obtener colores
            ViewBag.Colores = getColors(partida.CantidadJugadores * 2);
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

        private string[] getColors(int? cantidad)
        {
            //genera colores hexadesimales aleatorios
            Random randomGen = new Random();
            string[] colors = new string[(int) cantidad];
            for (int i = 0; i < cantidad; i++)
            {
                colors[i] = String.Format("#{0:X6}", randomGen.Next(0x1000000));
            }
            return colors;
        }
    }
}