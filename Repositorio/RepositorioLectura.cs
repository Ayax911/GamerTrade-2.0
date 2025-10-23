using System.Data;
using Microsoft.Data.SqlClient;
using APiGamer.Repositorio.Abstracciones;
using APiGamer.Repositorio.Compartido;

namespace APiGamer.Repositorio
{
 
    public class RepositorioLectura : RepositorioBase, IRepositorioLectura
    {
        private readonly IConexionFactory _conexionFactory;
        private readonly ILogger<RepositorioLectura> _logger;

        public RepositorioLectura(
            IConexionFactory conexionFactory,
            ILogger<RepositorioLectura> logger) : base(conexionFactory)
        {
            _conexionFactory = conexionFactory ?? 
                throw new ArgumentNullException(nameof(conexionFactory));
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
            
            _logger.LogInformation(
                "RepositorioLectura inicializado con: {TipoBD}", 
                _conexionFactory.TipoBaseDatos
            );
        }
        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerFilasAsync(
            string nombreTabla,
            string? esquema,
            int? limite)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException(
                    "El nombre de la tabla no puede estar vacío.", 
                    nameof(nombreTabla)
                );

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            int limiteFinal = limite ?? 1000;

            string sql = $"SELECT TOP ({limiteFinal}) * FROM [{esquemaFinal}].[{nombreTabla}]";

            _logger.LogDebug(
                "Ejecutando consulta: {Sql} (Límite: {Limite})", 
                sql, 
                limiteFinal
            );

            try
            {
                // USO DE FACTORY
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);
                using var lector = await comando.ExecuteReaderAsync();
                
                var resultado = await ConvertirAListaAsync(lector);
                
                _logger.LogInformation(
                    "Consulta exitosa en {Tabla}: {Filas} filas obtenidas", 
                    $"{esquemaFinal}.{nombreTabla}", 
                    resultado.Count
                );
                
                return resultado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error SQL al consultar tabla {Tabla}", 
                    $"{esquemaFinal}.{nombreTabla}"
                );
                throw CrearExcepcionSql(
                    $"Error al consultar tabla {esquemaFinal}.{nombreTabla}", 
                    ex
                );
            }
        }

        
        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valor)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla) || string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("Tabla o clave no pueden estar vacías.");

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            string sql = $"SELECT * FROM [{esquemaFinal}].[{nombreTabla}] WHERE [{nombreClave}] = @valor";

            _logger.LogDebug(
                "Buscando en {Tabla} donde {Clave} = {Valor}", 
                $"{esquemaFinal}.{nombreTabla}", 
                nombreClave, 
                valor
            );

            try
            {
              
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@valor", valor));
                
                using var lector = await comando.ExecuteReaderAsync();
                var resultado = await ConvertirAListaAsync(lector);
                
                _logger.LogInformation(
                    "Búsqueda exitosa: {Registros} registros encontrados", 
                    resultado.Count
                );
                
                return resultado;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error al filtrar {Tabla} por {Clave}", 
                    nombreTabla, 
                    nombreClave
                );
                throw CrearExcepcionSql(
                    $"Error al filtrar {nombreTabla} por {nombreClave}", 
                    ex
                );
            }
        }

        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException(
                    "El nombre de la tabla no puede estar vacío.", 
                    nameof(nombreTabla)
                );
            if (datos == null || !datos.Any())
                throw new ArgumentException(
                    "Los datos no pueden estar vacíos.", 
                    nameof(datos)
                );

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            EncriptarCampos(datosFinales, camposEncriptar);

            var columnas = string.Join(", ", datosFinales.Keys.Select(k => $"[{k}]"));
            var parametros = string.Join(", ", datosFinales.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO [{esquemaFinal}].[{nombreTabla}] ({columnas}) VALUES ({parametros})";

            _logger.LogDebug(
                "Insertando en {Tabla} con {Campos} campos", 
                $"{esquemaFinal}.{nombreTabla}", 
                datosFinales.Count
            );

            try
            {
               
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                    comando.Parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value));

                int filas = await comando.ExecuteNonQueryAsync();
                
                _logger.LogInformation(
                    "Registro creado exitosamente en {Tabla}", 
                    $"{esquemaFinal}.{nombreTabla}"
                );
                
                return filas > 0;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error al insertar en {Tabla}", 
                    nombreTabla
                );
                throw CrearExcepcionSql($"Error al insertar en {nombreTabla}", ex);
            }
        }

 
        public async Task<int> ActualizarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla) || string.IsNullOrWhiteSpace(nombreClave))
                throw new ArgumentException("La tabla o clave no pueden estar vacías.");
            if (datos == null || !datos.Any())
                throw new ArgumentException(
                    "Los datos no pueden estar vacíos.", 
                    nameof(datos)
                );

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            EncriptarCampos(datosFinales, camposEncriptar);

            var setClause = string.Join(", ", datosFinales.Keys.Select(k => $"[{k}] = @{k}"));
            string sql = $"UPDATE [{esquemaFinal}].[{nombreTabla}] SET {setClause} WHERE [{nombreClave}] = @valorClave";

            _logger.LogDebug(
                "Actualizando {Tabla} donde {Clave} = {Valor}", 
                $"{esquemaFinal}.{nombreTabla}", 
                nombreClave, 
                valorClave
            );

            try
            {
               
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                    comando.Parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value));

                comando.Parameters.Add(new SqlParameter("@valorClave", valorClave));

                int filasAfectadas = await comando.ExecuteNonQueryAsync();
                
                _logger.LogInformation(
                    "Actualización completada: {Filas} filas afectadas en {Tabla}", 
                    filasAfectadas, 
                    $"{esquemaFinal}.{nombreTabla}"
                );
                
                return filasAfectadas;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error al actualizar {Tabla}", 
                    nombreTabla
                );
                throw CrearExcepcionSql($"Error al actualizar {nombreTabla}", ex);
            }
        }

  
        public async Task<int> EliminarAsync(
            string nombreTabla,
            string? esquema,
            string nombreClave,
            string valorClave)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException(
                    "El nombre de la tabla no puede estar vacío.", 
                    nameof(nombreTabla)
                );

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            string sql = $"DELETE FROM [{esquemaFinal}].[{nombreTabla}] WHERE [{nombreClave}] = @valorClave";

            _logger.LogWarning(
                "Eliminando de {Tabla} donde {Clave} = {Valor}", 
                $"{esquemaFinal}.{nombreTabla}", 
                nombreClave, 
                valorClave
            );

            try
            {
           
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@valorClave", valorClave));
                
                int filasEliminadas = await comando.ExecuteNonQueryAsync();
                
                _logger.LogInformation(
                    "Eliminación completada: {Filas} filas eliminadas de {Tabla}", 
                    filasEliminadas, 
                    $"{esquemaFinal}.{nombreTabla}"
                );
                
                return filasEliminadas;
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error al eliminar en {Tabla}", 
                    nombreTabla
                );
                throw CrearExcepcionSql($"Error al eliminar en {nombreTabla}", ex);
            }
        }

        
        public async Task<string?> ObtenerHashContrasenaAsync(
            string nombreTabla,
            string? esquema,
            string campoUsuario,
            string campoContrasena,
            string valorUsuario)
        {
            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            string sql = $"SELECT [{campoContrasena}] FROM [{esquemaFinal}].[{nombreTabla}] WHERE [{campoUsuario}] = @usuario";

            _logger.LogDebug(
                "Obteniendo hash de contraseña para usuario en {Tabla}", 
                $"{esquemaFinal}.{nombreTabla}"
            );

            try
            {
             
                using var conexion = (SqlConnection)_conexionFactory.CrearConexion();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@usuario", valorUsuario));

                var resultado = await comando.ExecuteScalarAsync();
                
                if (resultado != null)
                {
                    _logger.LogInformation(
                        "Hash de contraseña obtenido exitosamente para usuario en {Tabla}", 
                        $"{esquemaFinal}.{nombreTabla}"
                    );
                }
                else
                {
                    _logger.LogWarning(
                        "No se encontró usuario en {Tabla}", 
                        $"{esquemaFinal}.{nombreTabla}"
                    );
                }
                
                return resultado?.ToString();
            }
            catch (SqlException ex)
            {
                _logger.LogError(
                    ex, 
                    "Error al obtener hash de contraseña en {Tabla}", 
                    nombreTabla
                );
                throw CrearExcepcionSql(
                    $"Error al obtener hash de contraseña en {nombreTabla}", 
                    ex
                );
            }
        }
    }
}