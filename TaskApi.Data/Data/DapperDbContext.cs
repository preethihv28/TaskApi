using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TaskApi.Data
{
    public class DapperDbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DapperDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration["ConnectionStrings:DefaultConnection"].ToString();
        }

        public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    }
}
