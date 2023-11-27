namespace SnakeGameBackend.Models
{
    public class Jugador
    {
        public int Id { get; set; }
        public string? Nickname { get; set; }
        public string? ColorSerpiente { get; set; }
        public int? LargoSerpiente { get; set; }
        public string? Direccion { get; set; }
        public int PosicionX { get; set; }  // Agrega estas propiedades
        public int PosicionY { get; set; }
    }
}
