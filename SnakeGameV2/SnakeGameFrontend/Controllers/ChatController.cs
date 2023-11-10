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
    public class ChatController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public static Dictionary<int, string> Rooms =
            new()
            {
                {1, "sala1" },
                {2, "sala2" },
                {3, "sala3" },
                {4, "sala4" },
                {5, "sala5" },
                {6, "sala6" },
                {7, "sala7" },
                {8, "sala8" },
                {9, "sala9" },
                {10, "sala10" }
            };
        public ChatController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Room(int room)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return View("Room", room);
        }
    }
}