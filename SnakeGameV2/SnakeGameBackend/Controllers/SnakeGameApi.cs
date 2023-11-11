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
        public IEnumerable<Partida> InsertarPartida(int tipo, int extension, int tematica,string codigoIdentificador, string nikName)
        {
            MssqlSingleton mssqlSingleton = MssqlSingleton.GetInstance(_configuration);
            Partida partida = new Partida();
            using (var connection = mssqlSingleton.GetConnection())
            {
                //
                // Abrir la conexión
                connection.Open();

                // Crear el comando SQL de inserción
                string insertQuery = "INSERT INTO Partidas (CodigoIdentificador, TipoJuego, Tematica, TiempoRestante, LargoObjetivo, Estado) " +
                                     "VALUES (@CodigoIdentificador, @Tipo, @Tematica, " +
                                     "CASE WHEN @Tipo = 1 THEN @Extension ELSE NULL END, " +
                                     "CASE WHEN @Tipo = 2 THEN @Extension ELSE NULL END, " +
                                     "0)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    // Agregar parámetros
                    command.Parameters.AddWithValue("@CodigoIdentificador", codigoIdentificador);
                    command.Parameters.AddWithValue("@Tipo", tipo);
                    command.Parameters.AddWithValue("@Extension", extension);
                    command.Parameters.AddWithValue("@Tematica", tematica);

                    // Ejecutar el comando
                    try
                    {
                        command.ExecuteNonQuery();
                        Console.WriteLine("Inserción exitosa.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al insertar: " + ex.Message);
                    }
                }
            }
            // contar la cantidad de partidas en estado 0
            int cantidad = 0;
            using (var connection = mssqlSingleton.GetConnection())
            {
                //
                // Abrir la conexión
                connection.Open();

                // Crear el comando SQL de inserción
                string insertQuery = "SELECT COUNT(*) FROM Partidas WHERE Estado = 0";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    // Ejecutar el comando
                    try
                    {
                        cantidad = (int)command.ExecuteScalar();
                        Console.WriteLine("Inserción exitosa.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al insertar: " + ex.Message);
                    }
                }
            }
            // generar Partida, si tipo es 1, extension es tiempo, si tipo es 2, extension es largo
            partida.CodigoIdentificador = codigoIdentificador;
            partida.Tipo = tipo;
            partida.Tematica = tematica;
            partida.Tiempo = tipo == 1 ? extension : null;
            partida.Largo = tipo == 2 ? extension : null;
            partida.Cantidad = cantidad;
            partida.Estado = 0;
            partida.Jugador = GetPlayer(nikName);
            partida.JugadorId = GetPlayer(nikName).Id;
            // retornar partida
            return new List<Partida> { partida};

        }

    }
}