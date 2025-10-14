using APiGamer.Repositorio.Abstracciones;
using APiGamer.Servicios.Abstracciones;
using Microsoft.Data.SqlClient;
using System.Data;
namespace APiGamer.Repositorio
{
    public class RepositorioConsulta : IRepositorioConsulta
    {
        readonly IProvedor _provedor;
        public RepositorioConsulta(IProvedor provedor)
        {
            _provedor = provedor ?? throw new ArgumentNullException(
               nameof(provedor),
               "IProvedor no puede ser null. Verificar configuración de inyección de dependencias en Program.cs."
           );
        }
        public Task<DataTable> EjecturaConsultaParametrizada(string consulta, Dictionary<string, object> parametros)
        {
            if (string.IsNullOrEmpty(consulta))
            {
                throw new ArgumentException("La consulta no puede ser nula o vacía.", nameof(consulta));
            }
            if (parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros), "El diccionario de parámetros no puede ser null.");
            }
            var (Esvalida, mensaje) = ValidarConsulta(consulta, parametros).Result;
            if (!Esvalida)
            {
                throw new ArgumentException($"La consulta no es válida: {mensaje}", nameof(consulta));
            }
            try
            {
                _provedor.AbrirConexion();
                if (_provedor == null)
                {
                    throw new InvalidOperationException("El proveedor de base de datos no está inicializado.");
                }
                using (SqlConnection connection = _provedor.AbrirConexion())
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = consulta;
                        command.CommandType = CommandType.Text;
                        foreach (var parametro in parametros)
                        {
                            var sqlParameter = command.CreateParameter();
                            sqlParameter.ParameterName = parametro.Key;
                            sqlParameter.Value = parametro.Value ?? DBNull.Value;
                            command.Parameters.Add(sqlParameter);
                        }
                        using (var adapter = new SqlDataAdapter((SqlCommand)command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            _provedor.CerrarConexion();
                            return Task.FromResult(dataTable);

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar la consulta parametrizada.", ex);
            }
        }
        public Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object> parametros)
        {
            List<string> palabrasProhibidas =
        [
                "INSERT", "UPDATE", "DELETE",
                "DROP", "ALTER", "CREATE", "EXEC", "EXECUTE", "UNION", "UNION SELECT",
                "--", ";--", ";", "/*", "*/", "/*!", "*/", "#",
                "@@", "@", "CHAR", "CHAR(", "NCHAR", "VARCHAR", "NVARCHAR", "TEXT", "NTEXT",
                "CAST(", "CONVERT(", "DECLARE", "SET", "SELECT", "FROM", "WHERE", "OR", "AND",
                "LIKE", "IN", "IS NULL", "IS NOT NULL",
                "XP_", "SP_", "SP_MS", "SYS.", "SYSOBJECTS", "INFORMATION_SCHEMA", "INFORMATION_SCHEMA.TABLES",
                "OBJECT_ID(", "OBJECT_NAME(", "DB_ID(", "DATABASE()", "SCHEMA", "SCHEMA_NAME(",
                "GRANT", "REVOKE", "USE", "SHUTDOWN",
                "WAITFOR", "WAITFOR DELAY", "SLEEP(", "BENCHMARK(", "DELAY",
                "LOAD_FILE(", "INTO OUTFILE", "INTO DUMPFILE", "LOAD DATA", "SELECT INTO",
                "OPENROWSET", "OPENDATASOURCE", "OPENQUERY",
                "REPLACE", "TRUNCATE", "MERGE",
                "EXEC sp_executesql", "EXECUTE IMMEDIATE",
                "SYSTEM_USER", "CURRENT_USER", "USER()", "SESSION_USER",
                "PASSWORD", "HASHBYTES(", "CRYPT_GEN_RANDOM(",
                "CAST", "CONVERT", "INFORMATION_SCHEMA.COLUMNS",
                "/*", "*/", "<!--", "-->",  "/*", "*/",
                "0x", "0x27", "' OR '1'='1", "\" OR \"1\"=\"1", "' OR 1=1 --",
                "OR 1=1", "'; DROP TABLE", "'); DROP TABLE",
                "BENCHMARK(", "REGEXP", "RLIKE",
                "`", "\"", "'"];
            foreach (var palabra in palabrasProhibidas)
            {
                if (consulta.ToUpper().Contains(palabra))
                {
                    return Task.FromResult((false, $"La consulta contiene una palabra prohibida: {palabra}"));
                }
            }
            return Task.FromResult((true, "La consulta es válida."));
        }
        public Task<DataTable> EjecturaProcedimientoAlmacenado(string consulta, Dictionary<string, object> parametros)
        {
            parametros = parametros ?? new Dictionary<string, object>();
            if (string.IsNullOrEmpty(consulta))
            {
                throw new ArgumentException("El nombre del procedimiento almacenado no puede ser nulo o vacío.", nameof(consulta));
            }
            using (var coneccion = _provedor.AbrirConexion())
            {
                using (var command = coneccion.CreateCommand())
                {
                    command.CommandText = consulta;
                    command.CommandType = CommandType.StoredProcedure;
                    foreach (var parametro in parametros)
                    {
                        var SqlParameter = command.CreateParameter();
                        SqlParameter.ParameterName = parametro.Key;
                        SqlParameter.Value = parametro.Value ?? DBNull.Value;
                        command.Parameters.Add(SqlParameter);
                    }
                    using(var adapter = new SqlDataAdapter((SqlCommand)command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return Task.FromResult(dataTable);
                    }
                }
            }
        }
    }
}
