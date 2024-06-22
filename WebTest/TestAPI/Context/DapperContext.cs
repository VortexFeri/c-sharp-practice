using System.Data;
using Microsoft.Data.SqlClient;

namespace TestAPI.Context;

public class DapperContext(IConfiguration configuration)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly string? _connectionString = configuration.GetConnectionString("SqlConnection");

    public IDbConnection CreateConnection() => new SqlConnection(_connectionString);
}