using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SnakeGame.Dao
{
    public class MssqlSingleton
    {
        private static MssqlSingleton _instance;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        private MssqlSingleton(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetValue<string>("conStr");
        }

        public static MssqlSingleton GetInstance(IConfiguration configuration)
        {
            _instance ??= new MssqlSingleton(configuration);
            return _instance;
        }

        public SqlConnection GetConnection()
        {
            SqlConnection connection = new(_connectionString);
            try
            {
                connection.Open();
                return connection;
            }
            catch (Exception)
            {
                connection.Dispose();
                throw;
            }
        }
    }
}
