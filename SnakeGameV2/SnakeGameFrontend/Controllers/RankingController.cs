using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SnakeGameFrontend.Models;
using System.Diagnostics;

namespace SnakeGameFrontend.Controllers
{
    // solo puede ingresar si inicio sesion
    [Authorize]
    public class RankingController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public RankingController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            List<Ranking> ranking = new();
            string apiUrl = _configuration.GetValue<string>("apiUrl");

            using (var httpClient = new HttpClient())
            {
                // Utilizar Uri.EscapeDataString para codificar el parámetro en la URL
                using var response = await httpClient.GetAsync($"{apiUrl}/GetRanking");

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    ranking = JsonConvert.DeserializeObject<List<Ranking>>(apiResponse);
                }
            }

            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return View(ranking);
        }
    }
}