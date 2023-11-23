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
            if (ViewBag.Jugador == null)
            {
                return RedirectToAction("Index", "Auth");
            }
            IEnumerable<PartidaEnEspera>? partidas = await GetPartidasAsync();
            return View(partidas);
        }

        public async Task<IActionResult> Room(string room)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            IEnumerable<PartidaEnEspera>? partidas = await GetPartidasAsync();
            PartidaEnEspera? partida = partidas.Where(p => p.CodigoIdentificador == room).FirstOrDefault();
            partida.Jugadores = await getJugadoresAsync(partida.CodigoIdentificador);

            ViewBag.Partida = partida;

            if (partida.JugadoresConectados >= partida.CantidadJugadores)
            {
                return RedirectToAction("Index", "Home");
            }

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

        private async Task<IEnumerable<SnakeGameBackend.Models.Jugador>?> getJugadoresAsync(string codigoIdentificador)
        {
            IEnumerable<SnakeGameBackend.Models.Jugador>? jugadores = null;
            string apiUrl = _configuration.GetValue<string>("apiUrl");

            using (var httpClient = new HttpClient())
            {
                using var response = await httpClient.GetAsync($"{apiUrl}/JugadoresEnPartida?identificadorPartida={codigoIdentificador}");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    jugadores = JsonConvert.DeserializeObject<IEnumerable<SnakeGameBackend.Models.Jugador>>(apiResponse);
                }
            }
            return jugadores;
        }

        private string[] getColors(int? cantidad)
        {
            //genera colores hexadesimales aleatorios
            //Random randomGen = new Random();
            //string[] colors = new string[(int) cantidad];
            //for (int i = 0; i < cantidad; i++)
            //{
            //    //colors[i] = String.Format("#{0:X6}", randomGen.Next(0x1000000));
            //    //una lista de 5 colores
            //}
            //una lista de colores fijos
            string[] colors = new string[15];
            colors[0] = "#FF0000";
            colors[1] = "#00FF00";
            colors[2] = "#0000FF";
            colors[3] = "#FFFF00";
            colors[4] = "#00FFFF";
            colors[5] = "#FF00FF";
            colors[6] = "#FF8000";
            colors[7] = "#8000FF";
            colors[8] = "#00FF80";
            colors[9] = "#FF0080";
            colors[10] = "#0080FF";
            colors[11] = "#FF80FF";
            colors[12] = "#80FF00";
            colors[13] = "#8123FF";
            colors[14] = "#FF8080";


            return colors;
        }
    }
}