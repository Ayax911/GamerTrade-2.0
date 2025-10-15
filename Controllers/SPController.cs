using APiGamer.Servicios.Abstracciones;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APiGamer.Controllers
{
    [Route("api/SpController")]
    [ApiController]
    public class SPController : ControllerBase
    {
        private readonly IServicioConsultas _servicioConsultas;
        private readonly ILogger<SPController> _logger;
        public SPController(IServicioConsultas servicioConsultas, ILogger<SPController> logger)
        {
            _servicioConsultas = servicioConsultas;
            _logger = logger;
        }
        [HttpPost("EjecutarSP {NombreSp}")]
        public async Task<IActionResult> EjecutarProcedimientoAlmacenado(string NombreSp, [FromBody] Dictionary<string,object> parametros)
        {
             if
        }

    }
}
