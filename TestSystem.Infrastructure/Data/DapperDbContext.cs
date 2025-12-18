using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace TestSystem.Infrastructure.Data;

public class DapperDbContext
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DapperDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection");
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
    
    public IDbConnection CreateOpenConnection()
    {
        var connection = CreateConnection();
        connection.Open();
        return connection;
    }
}