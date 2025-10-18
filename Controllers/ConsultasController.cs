using System.Text.Json;
using APiGamer.Servicios.Abstracciones;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Authorization;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APiGamer.Controllers
{
    [Route("api/Consultas")]
    [Authorize]
    [ApiController]
    public class ConsultasController : ControllerBase
    {
         readonly IServicioConsultas _servicioConsultas;
         readonly ILogger<ConsultasController> _logger;
        public ConsultasController(IServicioConsultas servicioConsultas, ILogger<ConsultasController> logger)
        {
            _servicioConsultas = servicioConsultas;
            _logger = logger;
        }

        [HttpPost("EjecutarConsulta")]
        public async Task<IActionResult> EjecutarConsultaParametrizada([FromBody] Dictionary<string, object> CuerpoSolicitud)
        {
           
            try
            {
                if (!CuerpoSolicitud.TryGetValue("consulta", out var consultaOBJ) || consultaOBJ is null)
                {
                    return BadRequest("El campo 'consulta' es obligatorio.");
                }
                string consulta = consultaOBJ switch
                {
                    string texto => texto,
                    JsonElement json when json.ValueKind == JsonValueKind.String => json.GetString() ?? "",
                    _ => ""
                };
                if (string.IsNullOrEmpty(consulta))
                {
                    return BadRequest("El campo 'consulta' no puede estar vacío.");
                }
                Dictionary<string, object?> parametros = new Dictionary<string, object?>();
                if (CuerpoSolicitud.TryGetValue("parametros", out var parametrosobj) && parametrosobj is JsonElement json1
                    && json1.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in json1.EnumerateObject())
                    {
                        parametros[prop.Name] = prop.Value;
                    }
                }
                _logger.LogInformation(
                        "INICIO ejecución consulta SQL - Consulta: {Consulta}, Parámetros: {CantidadParametros}",
                        consulta.Length > 100 ? consulta.Substring(0, 100) + "..." : consulta,  // Truncar consultas muy largas en logs
                        parametros?.Count ?? 0                                        // Cantidad de parámetros recibidos
                    );
                var resultado = await _servicioConsultas.EjecutarConsultaParametrizadaDesdeJsonAsync(consulta, parametros);
                var list = new List<Dictionary<string, object>>();
                foreach (DataRow row in resultado.Rows)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (DataColumn col in resultado.Columns)
                    {
                        dict[col.ColumnName] = row[col];
                    }
                    list.Add(dict);
                }
                _logger.LogInformation(
                        "ÉXITO ejecución consulta SQL - Registros obtenidos: {Cantidad}",
                        list.Count     // Cantidad exacta de registros devueltos
                    );

                if (list.Count == 0)
                {
                    return NotFound("La consulta se ejecutó correctamente pero no devolvió registros.");
                }
                return Ok(new
                {
                    Registros = list,
                    Cantidad = list.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR al ejecutar consulta SQL: {Mensaje}", ex.Message);
                return StatusCode(500, $"Error interno del servidor al ejecutar la consulta: {ex.Message}");
            }
        }
    }
}
