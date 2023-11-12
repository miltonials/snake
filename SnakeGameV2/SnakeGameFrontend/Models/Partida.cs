﻿namespace SnakeGameFrontend.Models
{
    public class Partida
    {
        public int? Id { get; set; }
        public string? CodigoIdentificador { get; set; }
        public int? Tipo { get; set; }
        public int? Tiempo { get; set; }
        public int? Largo { get; set; }
        public int? Cantidad { get; set; }
        public int? Tematica { get; set; }
        public int? JugadorId { get; set; }
        public Jugador? Jugador { get; set; }
        public int Estado { get; internal set; }
    }
}