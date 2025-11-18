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
        private readonly ITokenServices _tokenServices;

        public AuthController(
            IServiciosCrud servicioCrud, 
            ITokenServices tokenServices)
        {
            _servicioCrud = servicioCrud ?? throw new ArgumentNullException(nameof(servicioCrud));
            _tokenServices = tokenServices;
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
            
            Console.WriteLine($"Intento de login: {valorUsuario} - Código: {codigo}");
            
            if(codigo == 200)
            {
                var token = _tokenServices.GenerarToken(valorUsuario);
                return Ok(new { mensaje, token });
            }

            return StatusCode(codigo, new { mensaje });
        }
    }
}
