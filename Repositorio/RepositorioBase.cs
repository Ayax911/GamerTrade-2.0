using System.Data;
using Microsoft.Data.SqlClient;
using APiGamer.Servicios.Abstracciones;

namespace APiGamer.Repositorio.Compartido
{

    public abstract class RepositorioBase
    {
        protected readonly IProvedor _provedor;

        protected RepositorioBase(IProvedor provedor)
        {
            _provedor = provedor ?? throw new ArgumentNullException(nameof(provedor));
        }

        protected async Task<SqlConnection> CrearConexionAsync()
        {
            var conexion = new SqlConnection(_provedor.ObtenerCadenaDeConexion());
            await conexion.OpenAsync();
            return conexion;
        }

        protected static async Task<List<Dictionary<string, object?>>> ConvertirAListaAsync(SqlDataReader lector)
        {
            var resultados = new List<Dictionary<string, object?>>();

            while (await lector.ReadAsync())
            {
                var fila = new Dictionary<string, object?>();
                for (int i = 0; i < lector.FieldCount; i++)
                {
                    fila[lector.GetName(i)] = lector.IsDBNull(i) ? null : lector.GetValue(i);
                }
                resultados.Add(fila);
            }

            return resultados;
        }


        protected static void EncriptarCampos(Dictionary<string, object?> datos, string? camposEncriptar)
        {
            if (string.IsNullOrWhiteSpace(camposEncriptar)) return;

            var campos = camposEncriptar.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(c => c.Trim());

            foreach (var campo in campos)
            {
                if (datos.ContainsKey(campo) && datos[campo] != null)
                {
                    string valor = datos[campo]!.ToString()!;
                    datos[campo] = BCrypt.Net.BCrypt.HashPassword(valor);
                }
            }
        }

        protected static InvalidOperationException CrearExcepcionSql(
            string mensajeBase,
            SqlException ex,
            string? contexto = null)
        {
            return new InvalidOperationException(
                $"{mensajeBase}. {(contexto != null ? $"Contexto: {contexto}. " : "")}" +
                $"Error SQL #{ex.Number}: {ex.Message}",
                ex
            );
        }
    }
}
