using Microsoft.AspNetCore.Mvc;
using APiGamer.Servicios.Abstracciones;
using Microsoft.AspNetCore.Authorization;

namespace APiGamer.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CrudController : ControllerBase
    {
        private readonly IServiciosCrud _servicioCrud;

        public CrudController(IServiciosCrud servicioCrud)
        {
            _servicioCrud = servicioCrud ?? throw new ArgumentNullException(nameof(servicioCrud));
        }


        [HttpGet("listar")]
        public async Task<IActionResult> Listar(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromQuery] int? limite)
        {
            var resultado = await _servicioCrud.ListarAsync(tabla, esquema, limite);
            return Ok(resultado);
        }

        [HttpGet("obtener")]
        public async Task<IActionResult> ObtenerPorClave(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromQuery] string clave,
            [FromQuery] string valor)
        {
            var resultado = await _servicioCrud.ObtenerPorClaveAsync(tabla, esquema, clave, valor);
            return Ok(resultado);
        }

  
        [HttpPost("crear")]
        public async Task<IActionResult> Crear(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromBody] Dictionary<string, object?> datos,
            [FromQuery] string? camposEncriptar)
        {
            bool exito = await _servicioCrud.CrearAsync(tabla, esquema, datos, camposEncriptar);
            return exito ? Ok("Registro creado correctamente") : BadRequest("No se pudo crear el registro");
        }

        [HttpPut("actualizar")]
        public async Task<IActionResult> Actualizar(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromQuery] string clave,
            [FromQuery] string valor,
            [FromBody] Dictionary<string, object?> datos,
            [FromQuery] string? camposEncriptar)
        {
            int filas = await _servicioCrud.ActualizarAsync(tabla, esquema, clave, valor, datos, camposEncriptar);
            return filas > 0 ? Ok($"Se actualizaron {filas} filas.") : NotFound("No se encontró el registro.");
        }

        [HttpDelete("eliminar")]
        public async Task<IActionResult> Eliminar(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromQuery] string clave,
            [FromQuery] string valor)
        {
            int filas = await _servicioCrud.EliminarAsync(tabla, esquema, clave, valor);
            return filas > 0 ? Ok($"Se eliminaron {filas} filas.") : NotFound("No se encontró el registro.");
        }
    }
}