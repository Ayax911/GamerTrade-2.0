// APiGamer/Repositorio/SqlServerConexionFactory.cs
using System.Data;
using Microsoft.Data.SqlClient;
using APiGamer.Repositorio.Abstracciones;

namespace APiGamer.Repositorio
{
 
    public class SqlServerConexionFactory : IConexionFactory
    {
        private readonly string _connectionString;
        private readonly ILogger<SqlServerConexionFactory> _logger;

        public SqlServerConexionFactory(
            IConfiguration configuration,
            ILogger<SqlServerConexionFactory> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(
                    "ConnectionString 'DefaultConnection' no configurado en appsettings.json"
                );
            _logger = logger;
        }

        public IDbConnection CrearConexion()
        {
            try
            {
                var conexion = new SqlConnection(_connectionString);
                conexion.Open();
                
                _logger.LogDebug("Conexión SQL Server creada exitosamente");
                
                return conexion;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error al crear conexión SQL Server");
                throw;
            }
        }

        public string TipoBaseDatos => "SQL Server";
    }
}