
// Principios SOLID aplicados:
// - SRP: Solo se encarga de coordinar la lógica de negocio CRUD.
// - DIP: Depende de IRepositorioCrud (abstracción), no de una clase concreta.
// - OCP: Se pueden agregar nuevas reglas o repositorios sin modificar este código.
// - LSP: Cumple con IServicioCrud y puede ser reemplazado por otra implementación.
// -------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using APiGamer.Servicios.Abstracciones;
using APiGamer.Repositorio.Abstracciones;
using BCrypt.Net;


namespace APiGamer.Servicios
{
    /// <summary>
    /// Servicio genérico CRUD que coordina la lógica de negocio entre los controladores y los repositorios.
    /// </summary>
    public class ServiciosCrud : IServiciosCrud
    {
        private readonly IRepositorioLectura _repositorioCrud;
        private readonly IConfiguration _configuration;
        private readonly string[] _tablasProhibidas;

        public ServiciosCrud(IRepositorioLectura repositorioCrud, IConfiguration configuration)
        {
            _repositorioCrud = repositorioCrud ?? throw new ArgumentNullException(nameof(repositorioCrud));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            // Cargar solo una vez las tablas prohibidas para mejorar rendimiento
            _tablasProhibidas = _configuration.GetSection("TablasProhibidas").Get<string[]>() ?? Array.Empty<string>();
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ListarAsync(
            string nombreTabla, string? esquema, int? limite)
        {
            ValidarTablaPermitida(nombreTabla);

            string? esquemaNormalizado = NormalizarTexto(esquema);
            int? limiteNormalizado = (limite is null || limite <= 0) ? null : limite;

            var filas = await _repositorioCrud.ObtenerFilasAsync(nombreTabla, esquemaNormalizado, limiteNormalizado);
            return filas;
        }

        public async Task<IReadOnlyList<Dictionary<string, object?>>> ObtenerPorClaveAsync(
            string nombreTabla, string? esquema, string nombreClave, string valor)
        {
            ValidarTablaPermitida(nombreTabla);
            ValidarNoVacio(nombreClave, nameof(nombreClave));
            ValidarNoVacio(valor, nameof(valor));

            string? esquemaNormalizado = NormalizarTexto(esquema);
            return await _repositorioCrud.ObtenerPorClaveAsync(nombreTabla, esquemaNormalizado, nombreClave.Trim(), valor.Trim());
        }

        public async Task<bool> CrearAsync(
            string nombreTabla, string? esquema,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            ValidarTablaPermitida(nombreTabla);
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos no pueden estar vacíos.", nameof(datos));

            string? esquemaNormalizado = NormalizarTexto(esquema);
            string? camposEncriptarNormalizados = NormalizarTexto(camposEncriptar);

            return await _repositorioCrud.CrearAsync(nombreTabla, esquemaNormalizado, datos, camposEncriptarNormalizados);
        }

        public async Task<int> ActualizarAsync(
            string nombreTabla, string? esquema,
            string nombreClave, string valorClave,
            Dictionary<string, object?> datos,
            string? camposEncriptar = null)
        {
            ValidarTablaPermitida(nombreTabla);
            ValidarNoVacio(nombreClave, nameof(nombreClave));
            ValidarNoVacio(valorClave, nameof(valorClave));
            if (datos == null || !datos.Any())
                throw new ArgumentException("Los datos a actualizar no pueden estar vacíos.", nameof(datos));

            string? esquemaNormalizado = NormalizarTexto(esquema);
            string? camposEncriptarNormalizados = NormalizarTexto(camposEncriptar);

            return await _repositorioCrud.ActualizarAsync(
                nombreTabla, esquemaNormalizado, nombreClave.Trim(), valorClave.Trim(), datos, camposEncriptarNormalizados);
        }

        public async Task<int> EliminarAsync(
            string nombreTabla, string? esquema,
            string nombreClave, string valorClave)
        {
            ValidarTablaPermitida(nombreTabla);
            ValidarNoVacio(nombreClave, nameof(nombreClave));
            ValidarNoVacio(valorClave, nameof(valorClave));

            string? esquemaNormalizado = NormalizarTexto(esquema);
            return await _repositorioCrud.EliminarAsync(nombreTabla, esquemaNormalizado, nombreClave.Trim(), valorClave.Trim());
        }

        public async Task<(int codigo, string mensaje)> VerificarContrasenaAsync(
            string nombreTabla, string? esquema,
            string campoUsuario, string campoContrasena,
            string valorUsuario, string valorContrasena)
        {
            ValidarTablaPermitida(nombreTabla);
            ValidarNoVacio(campoUsuario, nameof(campoUsuario));
            ValidarNoVacio(campoContrasena, nameof(campoContrasena));
            ValidarNoVacio(valorUsuario, nameof(valorUsuario));
            ValidarNoVacio(valorContrasena, nameof(valorContrasena));

            string? esquemaNormalizado = NormalizarTexto(esquema);
            string? hashAlmacenado = await _repositorioCrud.ObtenerHashContrasenaAsync(
                nombreTabla, esquemaNormalizado, campoUsuario.Trim(), campoContrasena.Trim(), valorUsuario.Trim()
            );

            if (hashAlmacenado == null)
                return (404, "Usuario no encontrado");

            bool valida = BCrypt.Net.BCrypt.Verify(valorContrasena, hashAlmacenado);

            return valida
                ? (200, "Credenciales válidas")
                : (401, "Contraseña incorrecta");
        }

        private void ValidarTablaPermitida(string nombreTabla)
        {
            ValidarNoVacio(nombreTabla, nameof(nombreTabla));

            if (_tablasProhibidas.Contains(nombreTabla, StringComparer.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException($"La tabla '{nombreTabla}' está restringida y no puede ser consultada o modificada.");
        }

        private static void ValidarNoVacio(string valor, string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException($"El valor '{nombreParametro}' no puede estar vacío.", nombreParametro);
        }

        private static string? NormalizarTexto(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
        }
    }
}
