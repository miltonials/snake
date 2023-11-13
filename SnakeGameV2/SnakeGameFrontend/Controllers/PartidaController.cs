using Microsoft.AspNetCore.Mvc;
using SnakeGameBackend.Models;
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
            Jugador jugador = AuthController.GetPlayerSession(_contextAccessor);
            ViewBag.Jugador = jugador;

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

            //validar que el usuario no este en sala, osea que no este asociado a una partida con estado 0
           

            // Generar códigoIdentificador
            string codigoIdentificador = Guid.NewGuid().ToString();
            int sala = 0;

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


                // Realizar la solicitud HTTP POST
                //https://localhost:7043/SnakeGameApi/InsertarPartida?tipo=1&extension=1&tematica=1&codigoIdentificador=XYZ999&nikName=sa
                string encodedCodigoIdentificador = Uri.EscapeDataString(codigoIdentificador);
                int idJugador = jugador.Id;
                using var response = await httpClient.PostAsync($"{apiUrl}/InsertarPartida?tipo={tipo}&extension={extension}&tematica={tematica}&codigoIdentificador={encodedCodigoIdentificador}&jugadorId={idJugador}&cantidad={cantidad}", null);
                if (response.IsSuccessStatusCode)
                {
                    // Leer la respuesta del servidor
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    // Puedes deserializar la respuesta si es necesario a int 
                    sala = JsonConvert.DeserializeObject<int>(apiResponse);
                    //partida = JsonConvert.DeserializeObject<Partida>(apiResponse);
                }
                else
                {
                    // Manejar errores si la solicitud no es exitosa
                    ViewBag.Error = $"Error al llamar a la API: {response.StatusCode}";
                    return View();
                }
            }
            string salaString = sala.ToString();

            // Redireccionar a la sala de chat con el ID de la sala
            // se usa el metodo Room de ChatController
            ViewBag.Jugador = jugador;
            // mostrar el jugador en la consola
            Console.WriteLine("jugador: " + jugador.Nickname);
            codigoIdentificador = codigoIdentificador.Substring(0, 10);
            return RedirectToAction("Room", "Chat", new { room = codigoIdentificador });
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
