using Microsoft.AspNetCore.SignalR;
using SnakeGameBackend.Models;
using SnakeGameFrontend.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SnakeGameFrontend.Models
{
    public class PlayHub : Hub
    {
        private static Dictionary<string, string> playerColors = new Dictionary<string, string>();
        private static HashSet<string> playersReady = new HashSet<string>();
        private List<List<int>> tablero;


        public async Task PrepararPartida(string room, string nickName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("CargarDatosJugador");
        }

        public async Task CreateBoard(string room)
        {
            IEnumerable<SnakeGameBackend.Models.Jugador> jugadores = await PlayController.GetRoomPlayers(room);
            Console.WriteLine(jugadores.Count() + " jugadores en la sala " + room);
            int nJugadores = jugadores.Count();
            int nCasillas = 10 * nJugadores;
            tablero = new List<List<int>>();


            // Inicializar tablero
            for (int i = 0; i < nCasillas; i++)
            {
                List<int> fila = new List<int>();
                for (int j = 0; j < nCasillas; j++)
                {
                    fila.Add(0);
                }
                tablero.Add(fila);
            }


            // Colocar serpientes de los jugadores en lugares aleatorios
            Random random = new Random();
            foreach (SnakeGameBackend.Models.Jugador jugador in jugadores)
            {
                int x = random.Next(0, nCasillas);
                int y = random.Next(0, nCasillas);
                if (tablero[x][y] != 0)
                {
                    while (tablero[x][y] != 0)
                    {
                        x = random.Next(0, nCasillas);
                        y = random.Next(0, nCasillas);
                    }
                    tablero[x][y] = jugador.Id;
                }
                else
                {
                    tablero[x][y] = jugador.Id;
                }
            }

            //colocar un alimento por cada jugador en casillas que estén vacías
            foreach (SnakeGameBackend.Models.Jugador jugador in jugadores)
            {
                int x = random.Next(0, nCasillas);
                int y = random.Next(0, nCasillas);
                if (tablero[x][y] != 0)
                {
                    while (tablero[x][y] != 0)
                    {
                        x = random.Next(0, nCasillas);
                        y = random.Next(0, nCasillas);
                    }
                    tablero[x][y] = -1;
                }
                else
                {
                    tablero[x][y] = -1;
                }
            }

            // Envía el tablero al cliente
            await Clients.Group(room).SendAsync("CargarTablero", tablero);
            /*
             la funcion CargarTablero es la siguiente:
            connection.on("CargarTablero", function (tablero) {
                console.log(tablero);
            });
             */
        }

        /*
         Juego: El área de juego debe disponer de lo siguiente:
            a) Un área para poder jugar(matriz), donde inicialmente se muestran las celdas y las serpientes de los jugadores (serpientes de largo 1) se colocan en un punto aleatorio.
            b) Por cada jugador se coloca un alimento en el tablero en una posición aleatoria. c) Una vez que inicia el juego el jugador va cambiando de direcciones, con las teclas de flechas, hacia donde se mueve la serpiente y el cuerpo se va desplazando por “el camino que ha pasado su cuerpo”.
            d) Cada vez que una serpiente se come un alimento aparece uno nuevo en un lugar aleatorio. e) Las serpientes van creciendo de tamaño con forme comen (crecen 1 largo de celda por cada alimento). Crecen hacia adelante, según su dirección.
            f) Las serpientes se mueven n movimientos por segundo, donde n será una variable, no alambrada, de sistema que se podrá configurar, puede iniciar en 3.
            g) Si la serpiente “choca” contra el borde de la matriz, contra ella misma u otra serpiente (se sabe quién chocó), la serpiente pierde n tamaños (configurado también), atrás. La serpiente se detiene y debe arrancar de nuevo al momento que el usuario indique una dirección. Puede que choquen de frente, ambos se penalizan.
            h) Se debe visualizar: a. un cronómetro que indique el tiempo restante o b. la cantidad del largo de serpiente que deben lograr.
            i) Nombre de cada jugador en la partida con el color de serpiente. j) Indicar gane: el sistema debe validar, terminado el tiempo o si alguien logra el largo, el
            ganador (puede existir empate).
         */

        public async Task IniciarPartida(string roomId)
        {
            //string apiUrl = _configuration.GetValue<string>("apiUrl");
            string apiUrl = ChatController._configuration.GetValue<string>("apiUrl");
            using var httpClient = new HttpClient();
            string encodedRoomId = Uri.EscapeDataString(roomId);
            string url = $"{apiUrl}/IniciarPartida?identificadorPartida={encodedRoomId}";
            //using var response = await httpClient.GetAsync(url);

            await Clients.Group(roomId).SendAsync("PartidaIniciada");
        }

        public async Task MoverJugadores(string roomId, List<object> jugadores, int[][] tableroAct)
        {
            Console.WriteLine("MOVIENDO JUGADORES");
            //convertir jugadores a lista de jugadores
            List<Jugador> jugadoresLista = new List<Jugador>();
            foreach (object jugador in jugadores)
            {
                Jugador jugadorNuevo = new Jugador();
                jugadorNuevo.Id = (int)jugador.GetType().GetProperty("Id").GetValue(jugador);
                jugadorNuevo.PosicionX = (int)jugador.GetType().GetProperty("PosicionX").GetValue(jugador);
                jugadorNuevo.PosicionY = (int)jugador.GetType().GetProperty("PosicionY").GetValue(jugador);
                jugadorNuevo.Direccion = (string)jugador.GetType().GetProperty("Direccion").GetValue(jugador);
                jugadoresLista.Add(jugadorNuevo);
            }

            foreach (Jugador jugador in jugadoresLista)
            {
                int x = jugador.PosicionX;
                int y = jugador.PosicionY;

                // Actualizar la posición según la dirección
                switch (jugador.Direccion)
                {
                    case "W": // Arriba
                        x--;
                        break;
                    case "A": // Izquierda
                        y--;
                        break;
                    case "S": // Abajo
                        x++;
                        break;
                    case "D": // Derecha
                        y++;
                        break;
                }

                // Verifica si el nuevo movimiento está dentro de los límites del tablero
                if (x >= 0 && x < tableroAct.Length && y >= 0 && y < tableroAct[0].Length)
                {
                    // Verifica si la nueva posición está vacía o contiene comida
                    if (tableroAct[x][y] == 0 || tableroAct[x][y] == -1)
                    {
                        // Mueve al jugador a la nueva posición en el tablero
                        tableroAct[x][y] = jugador.Id;
                        tableroAct[jugador.PosicionX][jugador.PosicionY] = 0;

                        // Actualiza la posición del jugador en el objeto Jugador
                        jugador.PosicionX = x;
                        jugador.PosicionY = y;
                    }
                    // Aquí puedes agregar lógica adicional para manejar otros escenarios (colisión con otras serpientes, etc.)
                }
                // Puedes manejar la situación en la que el nuevo movimiento está fuera de los límites del tablero si lo deseas
            }

            // Envía el tablero actualizado al cliente
            await Clients.Group(roomId).SendAsync("CargarTablero", tableroAct);
        }

    }
}
