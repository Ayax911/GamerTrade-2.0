using Microsoft.AspNetCore.Mvc;
using APiGamer.Servicios.Abstracciones;

namespace APiGamer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IServiciosCrud _servicioCrud;

        public AuthController(IServiciosCrud servicioCrud)
        {
            _servicioCrud = servicioCrud ?? throw new ArgumentNullException(nameof(servicioCrud));
        }

        [HttpPost("verificar")]
        public async Task<IActionResult> VerificarCredenciales(
            [FromQuery] string tabla,
            [FromQuery] string? esquema,
            [FromQuery] string campoUsuario,
            [FromQuery] string campoContrasena,
            [FromQuery] string valorUsuario,
            [FromQuery] string valorContrasena)
        {
            var (codigo, mensaje) = await _servicioCrud.VerificarContrasenaAsync(
                tabla, esquema, campoUsuario, campoContrasena, valorUsuario, valorContrasena);

            return StatusCode(codigo, new { mensaje });
        }
    }
}