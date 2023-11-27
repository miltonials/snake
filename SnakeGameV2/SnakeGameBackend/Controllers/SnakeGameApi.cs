using Microsoft.AspNetCore.Mvc;
using SnakeGame.Dao;
using System.Data.SqlClient;
using System.Data;
using SnakeGameBackend.Models;
using SnakeGameFrontend.Models;
using Microsoft.AspNetCore.Identity;

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
                registro.CodigoIdentificador = reader["CodigoIdentificador"].ToString();
                registro.Tipo = Convert.ToInt32(reader["TipoJuego"]);
                try
                {
                    registro.Tiempo = Convert.ToInt32(reader["TiempoRestante"]);
                }
                catch (Exception e)
                {
                    registro.Tiempo = 0;
                }
                try
                {
                    registro.Largo = Convert.ToInt32(reader["LargoObjetivo"]);
                }
                catch (Exception e)
                {
                    registro.Largo = 0;
                }

                //registro.Cantidad = Convert.ToInt32(reader["Cantidad"]);
                registro.Tematica = Convert.ToInt32(reader["Tematica"]);
                registro.Estado = Convert.ToInt32(reader["Estado"]);
                try
                {
                    registro.Cantidad = Convert.ToInt32(reader["CantidadJugadores"]);
                }
                catch (Exception e)
                {
                    registro.Cantidad = 2;  
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
                    JugadoresConectados = Convert.ToInt32(reader["JugadoresConectados"]),

                };
                partidas.Add(registro);
            }

            connection.Close();
            return partidas;
        }

        [HttpPost("UnirsePartida")]
        public void UnirsePartida(string identificadorPartida, string nickname, string colorSerpiente)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_UnirsePartida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdentificadorPartida", identificadorPartida);
            command.Parameters.AddWithValue("@Nickname", nickname);
            command.Parameters.AddWithValue("@ColorSerpiente", colorSerpiente);
            command.ExecuteNonQuery();
            connection.Close();
        }

        [HttpPost("AbandonarPartida")]
        public void AbandonarPartida(string identificadorPartida, string nickname)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_AbandonarPartida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdentificadorPartida", identificadorPartida);
            command.Parameters.AddWithValue("@Nickname", nickname);
            command.ExecuteNonQuery();
            connection.Close();
        }

        [HttpGet("JugadoresEnPartida")]
        public IEnumerable<Jugador> JugadoresEnPartida(string identificadorPartida)
        {
            List<Jugador> jugadoresEnPartida = new ();

            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_ObtenerJugadoresEnPartida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@IdentificadorPartida", identificadorPartida);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                Jugador jugador = new()
                {
                    Nickname = reader["Nickname"].ToString(),
                    ColorSerpiente = reader["ColorSerpiente"].ToString(),
                    LargoSerpiente = Convert.ToInt32(reader["LargoSerpiente"]),
                };
                jugadoresEnPartida.Add(jugador);
            }

            return jugadoresEnPartida;
        }

        /*
         sp_AsociarColorJugadorEnPartida (
        @CodigoIdentificador NVARCHAR(10),
        @Nickname NVARCHAR(50),
        @ColorSerpiente NVARCHAR(20)
         */
        [HttpPost("AsociarColorJugadorEnPartida")]
        public void AsociarColorJugadorEnPartida(string codigoIdentificador, string nickname, string colorSerpiente)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_AsociarColorJugadorEnPartida", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@CodigoIdentificador", codigoIdentificador);
            command.Parameters.AddWithValue("@Nickname", nickname);
            command.Parameters.AddWithValue("@ColorSerpiente", colorSerpiente);
            command.ExecuteNonQuery();
            connection.Close();
        }


        //sp_ObtenerJugadoresListos
        [HttpGet("jugadoresListos")]
        public int JugadoresListos(string codigoIdentificador)
        {
            int jugadoresListos = 0;

            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            using var connection = mssqlSingleton.GetConnection();
            SqlCommand command = new("sp_ObtenerJugadoresListos", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@CodigoIdentificador", codigoIdentificador);

            using SqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                jugadoresListos = Convert.ToInt32(reader["jugadoresListos"]);
            }

            return jugadoresListos;
        }
    }
}