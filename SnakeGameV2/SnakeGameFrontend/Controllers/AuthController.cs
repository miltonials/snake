﻿using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SnakeGameFrontend.Models;
using System;
using System.Security.Claims;
using System.Text;

namespace SnakeGameFrontend.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _contextAccessor;
        public AuthController(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _contextAccessor = httpContextAccessor;
        }
        public IActionResult Index()
        {
            ClaimsPrincipal c = _contextAccessor.HttpContext.User;
            if (c.Identity != null && c.Identity.IsAuthenticated)
            {
                ViewBag.Jugador = AuthController.GetPlayerSession(_contextAccessor);

                if (ViewBag.Jugador == null)
                {
                    _contextAccessor.HttpContext.User = null;
                    return View();
                }
                
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LogIn(string pNickname)
        {
            if (pNickname == null || pNickname == "")
            {
                // Mensaje que indica que no se ingresó la contraseña o el correo
                TempData["Mensaje"] = "No se ha ingresado la contraseña o el correo.";
                return RedirectToAction("Index");
            }

            SnakeGameBackend.Models.Jugador? jugador = await FindAsync(pNickname);

            // Guardar la información del usuario en una cookie
            List<Claim> c = new()
            {
                new Claim(ClaimTypes.Name, jugador.Nickname)
            };
            ClaimsIdentity ci = new ClaimsIdentity(c, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties ap = new();

            var usuarioJson = JsonConvert.SerializeObject(jugador);
            var cookieOptions = new CookieOptions
            {
                //Expires = DateTimeOffset.UtcNow.AddHours(1), // Define la expiración
                IsEssential = true // Marcar como esencial
            };
            Response.Cookies.Append("JugadorCookie", usuarioJson, cookieOptions);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(ci), ap);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogOut()
        {
            // Elimina la cookie de usuario
            Response.Cookies.Delete("JugadorCookie");

            // Limpia cualquier información relacionada con la sesión
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al inicio o a la página de inicio de sesión
            return RedirectToAction("Index", "Auth");
        }


        public async Task<SnakeGameBackend.Models.Jugador?> FindAsync(string pNickname)
        {
            SnakeGameBackend.Models.Jugador? jugador = null;
            string apiUrl = _configuration.GetValue<string>("apiUrl");

            using (var httpClient = new HttpClient())
            {
                // Utilizar Uri.EscapeDataString para codificar el parámetro en la URL
                string encodedNickname = Uri.EscapeDataString(pNickname);
                using var response = await httpClient.PostAsync($"{apiUrl}/LogIn?nickname={encodedNickname}", null);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    jugador = JsonConvert.DeserializeObject<SnakeGameBackend.Models.Jugador>(apiResponse);
                }
            }

            return jugador;
        }




        public static SnakeGameBackend.Models.Jugador? GetPlayerSession(IHttpContextAccessor httpContextAccessor)
        {
            HttpContext? httpContext = httpContextAccessor.HttpContext;
            SnakeGameBackend.Models.Jugador? jugador = null;

            if (httpContext.Request.Cookies.TryGetValue("JugadorCookie", out string usuarioJson))
            {
                jugador = JsonConvert.DeserializeObject<SnakeGameBackend.Models.Jugador>(usuarioJson);
            }

            return jugador;
        }
    }
}