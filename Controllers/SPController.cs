using APiGamer.Servicios.Abstracciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APiGamer.Controllers
{
    [Route("api/SpController")]
    [Authorize]
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
        [HttpPost("EjecutarSP/{NombreSp}")]
        public async Task<IActionResult> EjecutarProcedimientoAlmacenado(string NombreSp,
         [FromBody] Dictionary<string,object?>? parametros,[FromQuery] List<string> camposEncriptar)
        {
             if(string.IsNullOrEmpty(NombreSp))
            {
                return BadRequest("El nombre del procedimiento almacenado es obligatorio.");
            }
            var result =  await _servicioConsultas.EjecturaProcedimientoAlmacenadoAsync(NombreSp, parametros, camposEncriptar);
            if(result.Rows.Count == 0)
            {
                return NotFound("El procedimiento almacenado no devolvió resultados.");
            }
            var list = new List<Dictionary<string, object>>();
            foreach (DataRow row in result.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in result.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            if(list.Count == 0)
            {
                return NotFound("El procedimiento almacenado se ejecutó correctamente pero no devolvió registros.");
            }
            return Ok(new
            {
                resultado = list,
                mensaje = "Procedimiento almacenado ejecutado correctamente."
            });
        }

    }
}
