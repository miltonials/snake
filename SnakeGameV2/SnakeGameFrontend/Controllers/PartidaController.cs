using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnakeGameBackend.Controllers;
using SnakeGame.Dao;
using SnakeGameFrontend.Models;
using System.Text;

namespace SnakeGameFrontend.Controllers
{
    public class PartidaController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;

        public PartidaController(IConfiguration configuration, IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = contextAccessor;
        }
        // GET: PartidaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: PartidaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: PartidaController/Create
        public ActionResult Crear()
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            return View();
        }

        // POST: PartidaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(int tipo, int extension,int tematica,int cantidad)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);
            //validar que no este vacio
            if (tipo == 0 || extension == 0 || tematica == 0 || cantidad == 0)
            {
                ViewBag.Error = "Debe llenar todos los campos";
                return View();
            }
            //validar que la extension sea mayor a 2
            if (extension < 2)
            {
                ViewBag.Error = "La extension debe ser mayor a 2";
                return View();
            }
            // validar que la cantidad sea mayor a 2
            if (cantidad < 2)
            {
                ViewBag.Error = "La cantidad debe ser mayor a 2";
                return View();
            }
            // generar codigoIdentificador
            string codigoIdentificador = Guid.NewGuid().ToString();
            
            string apiUrl = _configuration.GetValue<string>("apiUrl");
            using (var httpClient = new HttpClient())
            {
                // Utilizar Uri.EscapeDataString para codificar el parámetro en la URL
                using var response = await httpClient.PostAsync($"{apiUrl}/InsertarPartida"´, );

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    //partida = JsonConvert.DeserializeObject<Partida>(apiResponse);
                }
            }

            // redireccionar a la sala chat con el id de la sala
            return RedirectToAction("Index", "Chat");
        }

        // GET: PartidaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PartidaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PartidaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PartidaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
