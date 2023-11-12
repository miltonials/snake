using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnakeGameBackend.Controllers;
using SnakeGame.Dao;
using SnakeGameFrontend.Models;
using System.Text;
using Newtonsoft.Json;

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
        public async Task<IActionResult> Crear(int tipo, int extension, int tematica, int cantidad)
        {
            ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);

            // Validar que no esté vacío
            if (tipo == 0 || extension == 0 || tematica == 0 || cantidad == 0)
            {
                ViewBag.Error = "Debe llenar todos los campos";
                return View();
            }

            // Validar que la extensión sea mayor a 2
            if (extension < 2)
            {
                ViewBag.Error = "La extensión debe ser mayor a 2";
                return View();
            }

            // Validar que la cantidad sea mayor a 2
            if (cantidad < 2)
            {
                ViewBag.Error = "La cantidad debe ser mayor a 2";
                return View();
            }

            // Generar códigoIdentificador
            string codigoIdentificador = Guid.NewGuid().ToString();

            string apiUrl = _configuration.GetValue<string>("apiUrl");
            using (var httpClient = new HttpClient())
            {
                // Crear la partidad para enviarla al servidor
                Partida data = new()
                {
                    CodigoIdentificador = codigoIdentificador,
                    Tipo = tipo,
                    Largo = extension,
                    Tiempo = extension,
                    Tematica = tematica,
                    Cantidad = cantidad,
                    Estado = 0,
                    Jugador = AuthController.GetPlayerSession(_contextAccessor),
                    JugadorId = AuthController.GetPlayerSession(_contextAccessor).Id
                };

                // Serializar el objeto como JSON
                var jsonData = JsonConvert.SerializeObject(data);

                // Crear el contenido de la solicitud HTTP
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Realizar la solicitud HTTP POST
                using var response = await httpClient.PostAsync($"{apiUrl}/InsertarPartida", content);

                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta del servidor
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    // Puedes deserializar la respuesta si es necesario
                    // partida = JsonConvert.DeserializeObject<Partida>(apiResponse);
                }
                else
                {
                    // Manejar errores si la solicitud no es exitosa
                    ViewBag.Error = $"Error al llamar a la API: {response.StatusCode}";
                    return View();
                }
            }

            // Redireccionar a la sala de chat con el ID de la sala
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
