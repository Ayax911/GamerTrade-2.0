// APiGamer/Repositorio/Compartido/RepositorioBase.cs
using System.Data;
using Microsoft.Data.SqlClient;
using APiGamer.Repositorio.Abstracciones;
using BCrypt.Net;

namespace APiGamer.Repositorio.Compartido
{
    /// <summary>
    /// Clase base para todos los repositorios con métodos comunes
    /// </summary>
    public abstract class RepositorioBase
    {
        private readonly IConexionFactory _conexionFactory;

        /// <summary>
        /// Constructor que recibe la factory de conexiones
        /// </summary>
        protected RepositorioBase(IConexionFactory conexionFactory)
        {
            _conexionFactory = conexionFactory ?? 
                throw new ArgumentNullException(nameof(conexionFactory));
        }

        /// <summary>
        /// Crea una conexión usando la factory (mantiene compatibilidad con código legacy)
        /// </summary>
        protected async Task<SqlConnection> CrearConexionAsync()
        {
            var conexion = (SqlConnection)_conexionFactory.CrearConexion();
            
            // Si la factory no abrió la conexión, abrirla
            if (conexion.State != ConnectionState.Open)
            {
                await conexion.OpenAsync();
            }
            
            return conexion;
        }

        /// <summary>
        /// Convierte un SqlDataReader a una lista de diccionarios
        /// </summary>
        protected async Task<IReadOnlyList<Dictionary<string, object?>>> ConvertirAListaAsync(
            SqlDataReader lector)
        {
            var resultado = new List<Dictionary<string, object?>>();

            while (await lector.ReadAsync())
            {
                var fila = new Dictionary<string, object?>();
                
                for (int i = 0; i < lector.FieldCount; i++)
                {
                    string nombreColumna = lector.GetName(i);
                    object? valor = lector.IsDBNull(i) ? null : lector.GetValue(i);
                    fila[nombreColumna] = valor;
                }
                
                resultado.Add(fila);
            }

            return resultado;
        }

        /// <summary>
        /// Encripta campos específicos usando BCrypt
        /// </summary>
        protected void EncriptarCampos(
            Dictionary<string, object?> datos, 
            string? camposEncriptar)
        {
            if (string.IsNullOrWhiteSpace(camposEncriptar))
                return;

            var campos = camposEncriptar
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .ToList();

            foreach (var campo in campos)
            {
                if (datos.ContainsKey(campo) && 
                    datos[campo] is string valorString && 
                    !string.IsNullOrEmpty(valorString))
                {
                    // Evitar re-encriptar hashes BCrypt existentes
                    if (!valorString.StartsWith("$2a$") && 
                        !valorString.StartsWith("$2b$") && 
                        !valorString.StartsWith("$2y$"))
                    {
                        datos[campo] = BCrypt.Net.BCrypt.HashPassword(
                            valorString, 
                            workFactor: 12
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Crea una excepción personalizada con información de SQL
        /// </summary>
        protected InvalidOperationException CrearExcepcionSql(
            string mensaje, 
            SqlException ex)
        {
            var mensajeCompleto = $"{mensaje}. " +
                $"Error SQL {ex.Number}: {ex.Message}. " +
                $"Procedimiento: {ex.Procedure ?? "N/A"}, " +
                $"Línea: {ex.LineNumber}";
            
            return new InvalidOperationException(mensajeCompleto, ex);
        }
    }
}