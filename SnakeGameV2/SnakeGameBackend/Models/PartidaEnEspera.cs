using SnakeGameFrontend.Models;

namespace SnakeGameBackend.Models
{
    public class PartidaEnEspera: Partida
    {
        public string? TipoJuego { get; set; }
        public new string? Tematica { get; set; }
        public int? CantidadJugadores { get; set; }
        public int? JugadoresConectados { get; set; }
    }
}
