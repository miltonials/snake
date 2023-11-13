using Microsoft.AspNetCore.Mvc;
using SnakeGame.Dao;
using System.Data.SqlClient;
using System.Data;
using SnakeGameBackend.Models;
using SnakeGameFrontend.Models;

namespace SnakeGameBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SnakeGameApi : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SnakeGameApi> _logger;

        public SnakeGameApi(ILogger<SnakeGameApi> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("LogIn")]
        public Jugador LogIn(string nickname)
        {
            Console.WriteLine(nickname);
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_InsertarJugador", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Nickname", nickname);
            command.ExecuteNonQuery();

            connection.Close();

            return GetPlayer(nickname);
        }

        [HttpGet("GetPlayers")]
        public IEnumerable<Jugador> GetPlayers()
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select * from Jugadores", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<Jugador> jugadores = new();
            while (reader.Read())
            {
                Jugador registro = new()
                {
                    Id = Convert.ToInt32(reader["JugadorId"]),
                    Nickname = reader["Nickname"].ToString()
                };
                jugadores.Add(registro);
            }

            connection.Close();
            return jugadores;
        }

        //una funcion que busque un Jugador especifico
        [HttpGet("GetPlayer")]
        public Jugador GetPlayer(string nickname)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select * from Jugadores where Nickname = @nickname", connection);
            command.Parameters.AddWithValue("@nickname", nickname);
            SqlDataReader reader = command.ExecuteReader();
            Jugador registro = new();
            while (reader.Read())
            {
                registro.Id = Convert.ToInt32(reader["JugadorId"]);
                registro.Nickname = reader["Nickname"].ToString();
            }

            connection.Close();
            return registro;
        }

        [HttpGet("GetRanking")]
        public IEnumerable<Ranking> GetRanking()
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select * from ranking", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<Ranking> ranking = new();
            while (reader.Read())
            {
                Ranking registro = new()
                {
                    Ganador = reader["Ganador"].ToString(),
                    Largo = reader["Largo"].ToString(),
                    Tematica = reader["Tematica"].ToString(),
                    Tipo = reader["Tipo"].ToString(),
                    IdentificadorPartida = reader["IdentificadorPartida"].ToString()
                };
                ranking.Add(registro);
            }
            connection.Close();
            return ranking;
        }

        
        [HttpPost("InsertarPartida")]
        public int InsertarPartida(int tipo, int extension, int tematica,string codigoIdentificador, int jugadorId, int cantidad)
        {
            Partida partida = new();
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_InsertarPartida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@TipoJuego", tipo);
            command.Parameters.AddWithValue("@Extension", extension);
            command.Parameters.AddWithValue("@Tematica", tematica);
            command.Parameters.AddWithValue("@CodigoIdentificador", codigoIdentificador);
            command.Parameters.AddWithValue("@JugadorID", jugadorId);
            command.Parameters.AddWithValue("@Cantidad", cantidad);
            command.ExecuteNonQuery();
            connection.Close();

            //sacar id de la partida
            return CantidaPartidaEnProceso();

            //return GetPartida(codigoIdentificador);

        }

        [HttpGet("GetPartida")]
        public Partida GetPartida(string codigoIdentificador)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select * from Partidas where CodigoIdentificador = @codigoIdentificador", connection);
            command.Parameters.AddWithValue("@codigoIdentificador", codigoIdentificador);
            SqlDataReader reader = command.ExecuteReader();
            Partida registro = new();
            while (reader.Read())
            {
                registro.Id = Convert.ToInt32(reader["PartidaId"]);
                registro.Tipo = Convert.ToInt32(reader["TipoJuego"]);
                try
                {
                    registro.Largo = Convert.ToInt32(reader["TiempoRestante"]);
                }
                catch (Exception e)
                {
                    registro.Largo = Convert.ToInt32(reader["LargoObjetivo"]);
                }
                registro.Tematica = Convert.ToInt32(reader["Tematica"]);
                registro.CodigoIdentificador = reader["CodigoIdentificador"].ToString();
                registro.Estado = Convert.ToInt32(reader["Estado"]);
                // try para que no se caiga si no hay cantidad de jugadores
                try
                {
                    registro.Cantidad = Convert.ToInt32(reader["CantidadJugadores"]);
                }
                catch (Exception e)
                {
                    registro.Cantidad = 0;
                }
            }

            connection.Close();
            return registro;
        }

        [HttpGet("CantidaPartidaEnProceso")]
        public int CantidaPartidaEnProceso()
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select count(*) as cantidad from Partidas where Estado = 0", connection);
            SqlDataReader reader = command.ExecuteReader();
            int cantidad = 0;
            while (reader.Read())
            {
                cantidad = Convert.ToInt32(reader["cantidad"]);
            }

            connection.Close();
            return cantidad;
        }

        [HttpGet("PartidasEnProceso")]
        public IEnumerable<PartidaEnEspera> PartidasEnProceso()
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("select * from PartidasEnEspera", connection);
            SqlDataReader reader = command.ExecuteReader();
            List<PartidaEnEspera> partidas = new();
            while (reader.Read())
            {
                PartidaEnEspera registro = new()
                {
                    Id = Convert.ToInt32(reader["PartidaId"]),
                    TipoJuego = reader["TipoJuego"].ToString(),
                    //Largo = Convert.ToInt32(reader["TiempoRestante"]),
                    Tematica = reader["Tematica"].ToString(),
                    CodigoIdentificador = reader["CodigoIdentificador"].ToString(),
                    CantidadJugadores = Convert.ToInt32(reader["CantidadJugadores"]),
                    JugadoresConectados = Convert.ToInt32(reader["JugadoresConectados"])
                };
                partidas.Add(registro);
            }

            connection.Close();
            return partidas;
        }
    }
}