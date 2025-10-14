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
          
            return await Task.FromResult((true, "La consulta es válida."));
        }
        public async Task<DataTable> EjecturaProcedimientoAlmacenado(string NombreSp, Dictionary<string, object> parametros)
        {
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
                        _provedor.CerrarConexion();
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
