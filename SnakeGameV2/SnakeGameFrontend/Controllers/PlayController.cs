using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Index()
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            //conseguir numero de jugadores
            ViewBag.CantidadJugadores = 2;
            return View();
        }
    }
}
