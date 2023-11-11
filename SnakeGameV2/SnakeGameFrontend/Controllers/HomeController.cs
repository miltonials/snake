using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnakeGameFrontend.Models;
using System.Diagnostics;

namespace SnakeGameFrontend.Controllers
{
    // solo puede ingresar si inicio sesion
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public HomeController(ILogger<HomeController> logger, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public IActionResult Index()
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // redirecciona a la pagina de crear partida
        public IActionResult CreateGame()
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return RedirectToAction("Crear", "Partida");
        }

    }
}