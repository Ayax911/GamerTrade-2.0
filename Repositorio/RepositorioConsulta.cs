/*using APiGamer.Repositorio.Abstracciones;
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
        public async Task<DataTable> EjecturaConsultaParametrizada(string consulta, Dictionary<string, object>? parametros)
        { // SELECT * FROM USUARIOS WHERE NOMBRE = @nombre
            // [@nombre:kewin]
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
                        agregarParametros((SqlCommand)command, parametros);
                        using (var adapter = new SqlDataAdapter((SqlCommand)command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return await Task.FromResult(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                string mensajeError = ex.Number switch
                {
                    2 => "Timeout: La consulta tardó demasiado en ejecutarse",
                    207 => "Nombre de columna inválido en la consulta SQL",
                    208 => "Tabla o vista especificada no existe en la base de datos",
                    102 => "Error de sintaxis en la consulta SQL",
                    515 => "Valor null no permitido en columna que no acepta nulls",
                    547 => "Violación de restricción de clave foránea",
                    2812 => "Procedimiento almacenado no encontrado",
                    8152 => "String or binary data would be truncated (datos demasiado largos)",
                    2146 => "Error de conversión de tipos de datos",
                    _ => $"Error SQL Server (Código {ex.Number}): {ex.Message}"
                };
                throw new InvalidOperationException(
                    $"Error al ejecutar consulta SQL: {mensajeError}. Consulta: {consulta}",
                    ex
                );
            }
        }
        public async Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object>? parametros)
        {
            try
            {
                using (var conexion = _provedor.AbrirConexion())
                using (var command = conexion.CreateCommand())
                {
                    command.CommandText = "sys.sp_describe_first_result_set";
                    command.CommandType = CommandType.StoredProcedure;

                    // Pasamos la consulta como parámetro
                    var sqlParam = new SqlParameter("@tsql", SqlDbType.NVarChar)
                    {
                        Value = consulta
                    };
                    command.Parameters.Add(sqlParam);

                    // Parámetros adicionales requeridos por el SP
                    command.Parameters.Add(new SqlParameter("@params", SqlDbType.NVarChar) { Value = DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@browse_information_mode", SqlDbType.Int) { Value = 0 });

                    await command.ExecuteNonQueryAsync();

                    return (true, "Consulta válida");
                }
            }
            catch (SqlException sqlEx)
            {
                // MAPEO ESPECÍFICO DE ERRORES DE VALIDACIÓN SQL SERVER
                string mensajeError = sqlEx.Number switch
                {
                    102 => "Error de sintaxis SQL: revise la estructura de la consulta",
                    207 => "Nombre de columna inválido: verifique que las columnas existan",
                    208 => "Objeto no válido: tabla o vista no existe en la base de datos",
                    156 => "Palabra clave SQL incorrecta o en posición incorrecta",
                    170 => "Error de sintaxis cerca de palabra reservada",
                    _ => $"Error de validación SQL Server (Código {sqlEx.Number}): {sqlEx.Message}"
                };

                return (false, mensajeError);
            }
            catch (Exception ex)
            {
                return (false, $"Error inesperado en validación: {ex.Message}");
            }
        }
        public async Task<DataTable> EjecturaProcedimientoAlmacenado(string NombreSp, Dictionary<string, object>? parametros)
        {
            // EXEC NombreSp @param1, @param2
            parametros = parametros ?? new Dictionary<string, object>();
            using (var coneccion = _provedor.AbrirConexion())
            {
                using (var command = coneccion.CreateCommand())
                {
                    command.CommandText = NombreSp; 
                    command.CommandType = CommandType.StoredProcedure;
                    agregarParametros((SqlCommand)command, parametros);
                    using (var adapter = new SqlDataAdapter((SqlCommand)command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return await Task.FromResult(dataTable);
                    }
                }
            }
        }
        public static void agregarParametros(SqlCommand command, Dictionary<string, object>? parametros)
        {
           
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "El comando no puede ser null.");
            }
            if (parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros), "El diccionario de parámetros no puede ser null.");
            }
            foreach (var parametro in parametros)
            {
                command.Parameters.Add(CrearSQLParametroOptimizado(parametro.Key,parametro.Value));
            }
        }
        public static SqlParameter CrearSQLParametroOptimizado(string nombre, object? valor)
        {
            
            if (string.IsNullOrEmpty(nombre))
            {
                throw new ArgumentException("El nombre del parámetro no puede ser nulo o vacío.", nameof(nombre));
            }

            return valor switch
            {
                null => new SqlParameter(nombre, DBNull.Value),
                int intValue => new SqlParameter(nombre, SqlDbType.Int) { Value = intValue },
                long longValue => new SqlParameter(nombre, SqlDbType.BigInt) { Value = longValue },
                string strValue => new SqlParameter(nombre, SqlDbType.NVarChar, Math.Min(strValue.Length, 4000)) { Value = strValue },
                bool boolValue => new SqlParameter(nombre, SqlDbType.Bit) { Value = boolValue },
                DateTime dateTimeValue => new SqlParameter(nombre, SqlDbType.DateTime2) { Value = dateTimeValue },
                DateOnly dateOnlyValue => new SqlParameter(nombre, SqlDbType.Date) { Value= dateOnlyValue.ToDateTime(TimeOnly.MinValue)},
                TimeOnly timeOnlyValue => new SqlParameter(nombre, SqlDbType.Time) { Value = timeOnlyValue.ToTimeSpan() },
                decimal decimalValue => new SqlParameter(nombre, SqlDbType.Decimal) { Value = decimalValue },
                double doubleValue => new SqlParameter(nombre, SqlDbType.Float) { Value = doubleValue },
                float floatValue => new SqlParameter(nombre, SqlDbType.Real) { Value = floatValue },
                byte[] byteArrayValue => new SqlParameter(nombre, SqlDbType.VarBinary, Math.Min(byteArrayValue.Length, 8000)) { Value = byteArrayValue },
                Guid guidValue => new SqlParameter(nombre, SqlDbType.UniqueIdentifier) { Value = guidValue },
                _ => new SqlParameter(nombre, valor ?? DBNull.Value)
            };
        }
    }
}
*/

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
        public async Task<DataTable> EjecturaConsultaParametrizada(string consulta, Dictionary<string, object>? parametros)
        { // SELECT * FROM USUARIOS WHERE NOMBRE = @nombre
            // [@nombre:kewin]
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
                        agregarParametros((SqlCommand)command, parametros);
                        using (var adapter = new SqlDataAdapter((SqlCommand)command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return await Task.FromResult(dataTable);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                string mensajeError = ex.Number switch
                {
                    2 => "Timeout: La consulta tardó demasiado en ejecutarse",
                    207 => "Nombre de columna inválido en la consulta SQL",
                    208 => "Tabla o vista especificada no existe en la base de datos",
                    102 => "Error de sintaxis en la consulta SQL",
                    515 => "Valor null no permitido en columna que no acepta nulls",
                    547 => "Violación de restricción de clave foránea",
                    2812 => "Procedimiento almacenado no encontrado",
                    8152 => "String or binary data would be truncated (datos demasiado largos)",
                    2146 => "Error de conversión de tipos de datos",
                    _ => $"Error SQL Server (Código {ex.Number}): {ex.Message}"
                };
                throw new InvalidOperationException(
                    $"Error al ejecutar consulta SQL: {mensajeError}. Consulta: {consulta}",
                    ex
                );
            }
        }
        public async Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object>? parametros)
        {
            try
            {
                // ✅ Si es un EXEC (Stored Procedure), omitir validación con sp_describe_first_result_set
                // Los SP tienen su propia validación interna
                if (consulta.ToUpper().Trim().StartsWith("EXEC"))
                {
                    return await Task.FromResult((true, "Stored Procedure - validación omitida"));
                }

                using (var conexion = _provedor.AbrirConexion())
                using (var command = conexion.CreateCommand())
                {
                    command.CommandText = "sys.sp_describe_first_result_set";
                    command.CommandType = CommandType.StoredProcedure;

                    // Pasamos la consulta como parámetro
                    var sqlParam = new SqlParameter("@tsql", SqlDbType.NVarChar, -1) // -1 = MAX
                    {
                        Value = consulta
                    };
                    command.Parameters.Add(sqlParam);

                    // Parámetros adicionales requeridos por el SP
                    command.Parameters.Add(new SqlParameter("@params", SqlDbType.NVarChar) { Value = DBNull.Value });
                    command.Parameters.Add(new SqlParameter("@browse_information_mode", SqlDbType.TinyInt) { Value = 0 });

                    await command.ExecuteNonQueryAsync();

                    return (true, "Consulta válida");
                }
            }
            catch (SqlException sqlEx)
            {
                // MAPEO ESPECÍFICO DE ERRORES DE VALIDACIÓN SQL SERVER
                string mensajeError = sqlEx.Number switch
                {
                    102 => "Error de sintaxis SQL: revise la estructura de la consulta",
                    207 => "Nombre de columna inválido: verifique que las columnas existan",
                    208 => "Objeto no válido: tabla o vista no existe en la base de datos",
                    156 => "Palabra clave SQL incorrecta o en posición incorrecta",
                    170 => "Error de sintaxis cerca de palabra reservada",
                    2812 => "Procedimiento almacenado no encontrado",
                    _ => $"Error de validación SQL Server (Código {sqlEx.Number}): {sqlEx.Message}"
                };

                return (false, mensajeError);
            }
            catch (Exception ex)
            {
                // ⚠️ Si falla la validación por problemas de conexión u otros errores,
                // permitir que continúe (para no bloquear consultas válidas)
                Console.WriteLine($"⚠️ Warning en ValidarConsulta: {ex.Message}");
                return (true, $"Validación omitida: {ex.Message}");
            }
        }
        public async Task<DataTable> EjecturaProcedimientoAlmacenado(string NombreSp, Dictionary<string, object>? parametros)
        {
            // EXEC NombreSp @param1, @param2
            parametros = parametros ?? new Dictionary<string, object>();
            using (var coneccion = _provedor.AbrirConexion())
            {
                using (var command = coneccion.CreateCommand())
                {
                    command.CommandText = NombreSp; 
                    command.CommandType = CommandType.StoredProcedure;
                    agregarParametros((SqlCommand)command, parametros);
                    using (var adapter = new SqlDataAdapter((SqlCommand)command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return await Task.FromResult(dataTable);
                    }
                }
            }
        }
        public static void agregarParametros(SqlCommand command, Dictionary<string, object>? parametros)
        {
           
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), "El comando no puede ser null.");
            }
            if (parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros), "El diccionario de parámetros no puede ser null.");
            }
            foreach (var parametro in parametros)
            {
                command.Parameters.Add(CrearSQLParametroOptimizado(parametro.Key,parametro.Value));
            }
        }
        public static SqlParameter CrearSQLParametroOptimizado(string nombre, object? valor)
        {
            
            if (string.IsNullOrEmpty(nombre))
            {
                throw new ArgumentException("El nombre del parámetro no puede ser nulo o vacío.", nameof(nombre));
            }

            return valor switch
            {
                null => new SqlParameter(nombre, DBNull.Value),
                int intValue => new SqlParameter(nombre, SqlDbType.Int) { Value = intValue },
                long longValue => new SqlParameter(nombre, SqlDbType.BigInt) { Value = longValue },
                string strValue => new SqlParameter(nombre, SqlDbType.NVarChar, Math.Min(strValue.Length, 4000)) { Value = strValue },
                bool boolValue => new SqlParameter(nombre, SqlDbType.Bit) { Value = boolValue },
                DateTime dateTimeValue => new SqlParameter(nombre, SqlDbType.DateTime2) { Value = dateTimeValue },
                DateOnly dateOnlyValue => new SqlParameter(nombre, SqlDbType.Date) { Value= dateOnlyValue.ToDateTime(TimeOnly.MinValue)},
                TimeOnly timeOnlyValue => new SqlParameter(nombre, SqlDbType.Time) { Value = timeOnlyValue.ToTimeSpan() },
                decimal decimalValue => new SqlParameter(nombre, SqlDbType.Decimal) { Value = decimalValue },
                double doubleValue => new SqlParameter(nombre, SqlDbType.Float) { Value = doubleValue },
                float floatValue => new SqlParameter(nombre, SqlDbType.Real) { Value = floatValue },
                byte[] byteArrayValue => new SqlParameter(nombre, SqlDbType.VarBinary, Math.Min(byteArrayValue.Length, 8000)) { Value = byteArrayValue },
                Guid guidValue => new SqlParameter(nombre, SqlDbType.UniqueIdentifier) { Value = guidValue },
                _ => new SqlParameter(nombre, valor ?? DBNull.Value)
            };
        }
    }
}