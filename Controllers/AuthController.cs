using Microsoft.AspNetCore.Mvc;
using APiGamer.Servicios.Abstracciones;
using Microsoft.AspNetCore.Authorization;

namespace APiGamer.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IServiciosCrud _servicioCrud;
        private readonly ITokenServices tokenServices;
        private readonly ILogger _logger;

        public AuthController(IServiciosCrud servicioCrud, ITokenServices tokenServices, ILogger logger)
        {
            _servicioCrud = servicioCrud ?? throw new ArgumentNullException(nameof(servicioCrud));
            this.tokenServices = tokenServices;
            _logger = logger;
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
            _logger.LogInformation("Intento de autenticación para el usuario {Usuario} en la tabla {Tabla}: {Mensaje}",
                valorUsuario, tabla, mensaje);
            if(codigo == 200)
            {
            var token =  tokenServices.GenerarToken(valorUsuario);
                return Ok(new { mensaje,token });
            }

            return StatusCode(codigo, new { mensaje });
        }
    }
}