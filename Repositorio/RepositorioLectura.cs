using System.Data;
using Microsoft.Data.SqlClient;
using APiGamer.Repositorio.Abstracciones;
using APiGamer.Repositorio.Compartido;
using APiGamer.Servicios.Abstracciones;

namespace APiGamer.Repositorio
{

    public class RepositorioLectura : RepositorioBase, IRepositorioLectura
    {
        public RepositorioLectura(IProvedor provedor) : base(provedor) { }


        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerFilasAsync(
            string nombreTabla,
            string? esquema,
            int? limite)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            int limiteFinal = limite ?? 1000;

            string sql = $"SELECT TOP ({limiteFinal}) * FROM [{esquemaFinal}].[{nombreTabla}]";

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);
                using var lector = await comando.ExecuteReaderAsync();
                return await ConvertirAListaAsync(lector);
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSql($"Error al consultar tabla {esquemaFinal}.{nombreTabla}", ex);
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

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@valor", valor));
                using var lector = await comando.ExecuteReaderAsync();
                return await ConvertirAListaAsync(lector);
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSql($"Error al filtrar {nombreTabla} por {nombreClave}", ex);
            }
        }

        public async Task<bool> CrearAsync(
            string nombreTabla,
            string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            if (string.IsNullOrWhiteSpace(nombreTabla))
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            EncriptarCampos(datosFinales, camposEncriptar);

            var columnas = string.Join(", ", datosFinales.Keys.Select(k => $"[{k}]"));
            var parametros = string.Join(", ", datosFinales.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO [{esquemaFinal}].[{nombreTabla}] ({columnas}) VALUES ({parametros})";

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                    comando.Parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value));

                int filas = await comando.ExecuteNonQueryAsync();
                return filas > 0;
            }
            catch (SqlException ex)
            {
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
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();

            var datosFinales = new Dictionary<string, object?>(datos);
            EncriptarCampos(datosFinales, camposEncriptar);

            var setClause = string.Join(", ", datosFinales.Keys.Select(k => $"[{k}] = @{k}"));
            string sql = $"UPDATE [{esquemaFinal}].[{nombreTabla}] SET {setClause} WHERE [{nombreClave}] = @valorClave";

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);

                foreach (var kvp in datosFinales)
                    comando.Parameters.Add(new SqlParameter($"@{kvp.Key}", kvp.Value ?? DBNull.Value));

                comando.Parameters.Add(new SqlParameter("@valorClave", valorClave));

                return await comando.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
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
                throw new ArgumentException("El nombre de la tabla no puede estar vacío.", nameof(nombreTabla));

            string esquemaFinal = string.IsNullOrWhiteSpace(esquema) ? "dbo" : esquema.Trim();
            string sql = $"DELETE FROM [{esquemaFinal}].[{nombreTabla}] WHERE [{nombreClave}] = @valorClave";

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@valorClave", valorClave));
                return await comando.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
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

            try
            {
                await using var conexion = await CrearConexionAsync();
                using var comando = new SqlCommand(sql, conexion);
                comando.Parameters.Add(new SqlParameter("@usuario", valorUsuario));

                var resultado = await comando.ExecuteScalarAsync();
                return resultado?.ToString();
            }
            catch (SqlException ex)
            {
                throw CrearExcepcionSql($"Error al obtener hash de contraseña en {nombreTabla}", ex);
            }
        }
    }
}
