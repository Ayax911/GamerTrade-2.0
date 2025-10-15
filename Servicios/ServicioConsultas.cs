using APiGamer.Repositorio.Abstracciones;
using APiGamer.Servicios.Abstracciones;
using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
namespace APiGamer.Servicios
{
    public class ServicioConsultas : IServicioConsultas
    {
        private readonly IConfiguration _configuration;
        private readonly IRepositorioConsulta _repositorioConsulta;
        public ServicioConsultas(IConfiguration configuration, IRepositorioConsulta repositorioConsulta)
        {
            _configuration = configuration ?? throw new ArgumentNullException(
               nameof(configuration),
               "IConfiguration no puede ser null. Verificar configuración de inyección de dependencias en Program.cs."
           );
            _repositorioConsulta = repositorioConsulta ?? throw new ArgumentNullException(
               nameof(repositorioConsulta),
               "IRepositorioConsulta no puede ser null. Verificar configuración de inyección de dependencias en Program.cs."
           );
        }
        public async Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object>? parametros, string[] tablasProhibidas)
        {
            Console.WriteLine("Validando consulta: " + consulta);
            if (consulta == null)
            {
                return await Task.FromResult((false, "La consulta no puede ser nula."));
            }
            List<string> palabrasProhibidas =
      [
             "DROP", "ALTER", "DELETE", "INSERT", "UPDATE", "EXEC", "EXECUTE",
"UNION", "--", ";--", "/*", "*/", "xp_", "sp_", "shutdown", "sleep(", "benchmark("];
            foreach (var palabra in palabrasProhibidas)
            {
                if (consulta.ToUpper().Contains(palabra))
                {
                    return await Task.FromResult((false, $"La consulta contiene una palabra prohibida: {palabra}"));
                }
            }
            foreach (var tabla in tablasProhibidas)
            {
                if (consulta.ToUpper().Contains(tabla.ToUpper()))
                {
                    return await Task.FromResult((false, $"La consulta intenta acceder a una tabla prohibida: {tabla}"));
                }
            }
            if (!consulta.ToUpper().Trim().StartsWith("SELECT") && !consulta.ToUpper().Trim().StartsWith("WITH"))
            {
                return await Task.FromResult((false, "La consulta debe ser una consulta SELECT o una expresión común de tabla (CTE)."));
            }
            return await Task.FromResult((true, "Consulta válida"));
        }
        /// <summary>
        /// Ejecuta Consultas Parametrizadas de forma segura
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<DataTable> EjecturaConsultaParametrizadaAsync(string consulta, Dictionary<string, object>? parametros)
        {
            string[] tablasProhibidas = _configuration.GetSection("TablasProhibidas").Get<string[]>() ?? Array.Empty<string>();
            var (Esvalida, mensaje) =  ValidarConsulta(consulta, parametros, tablasProhibidas).Result;
            if (!Esvalida)
            {
                throw new ArgumentException($"La consulta no es válida: {mensaje}", nameof(consulta));
            }
            return  await  _repositorioConsulta.EjecturaConsultaParametrizada(consulta, parametros);
        }

        /// <summary>
        /// Ejecuta Procedimientos Almacenados de forma segura
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public async Task<DataTable> EjecturaProcedimientoAlmacenadoAsync(string NombreSp, Dictionary<string, object?>? parametros,List<string> CamposEncriptar)
        {
            if (string.IsNullOrWhiteSpace(NombreSp) ||
             !Regex.IsMatch(NombreSp.Trim(), @"^[a-zA-Z_][a-zA-Z0-9_]*$"))
            {
                Console.Write(NombreSp);
                throw new ArgumentException("EL nombre del procedimiento almacenado no es válido. Debe comenzar con una letra o guion bajo y contener solo caracteres alfanuméricos y guiones bajos.", nameof(NombreSp));
            }

            {
                var parametrosGenericos = ConvertirParametrosEncriptados(parametros, CamposEncriptar); 
                return await _repositorioConsulta.EjecturaProcedimientoAlmacenado(NombreSp, parametrosGenericos);
            }
             var parametrosConvertidos = ConvertirParametrosJson(parametros);
            return await _repositorioConsulta.EjecturaProcedimientoAlmacenado(NombreSp, parametrosConvertidos);
        }
        /// <summary>
        /// Ejecuta Consultas Parametrizadas desde Json de forma segura
        /// </summary>
        /// <param name="consulta"></param>
        /// <param name="parametros"></param>
        /// <returns></returns>
        public async Task<DataTable> EjecutarConsultaParametrizadaDesdeJsonAsync(
          string consulta,
          Dictionary<string, object?>? parametros)
        {
           var parametrosConvertidos = ConvertirParametrosJson(parametros);
           return await EjecturaConsultaParametrizadaAsync(consulta, parametrosConvertidos);
        }
        /// <summary>
        /// Convierte los parámetros de JSON a un diccionario con tipos adecuados para SQL
        /// </summary>
        /// <param name="Parametros"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private Dictionary<string, object> ConvertirParametrosJson(Dictionary<string,object?>? Parametros)
        {
            var parametrosG = new Dictionary<string, object>();
            if(Parametros == null || Parametros.Count == 0)
            {
                return parametrosG ;
            }
            foreach (var parametro in Parametros)
            {
                
                string nombre = parametro.Key.StartsWith("@") ? parametro.Key: "@" + parametro.Key ;
                    if(!Regex.IsMatch(nombre, @"^@\w+$"))
                    {
                        throw new ArgumentException($"El nombre del parámetro '{parametro.Key}' no es válido. Debe comenzar con '@' y contener solo caracteres alfanuméricos y guiones bajos.");
                    }
                 
                    object? valorTipado;  
                if(parametro.Value == null)
                {
                    valorTipado = null;
                }
                else if( parametro.Value is JsonElement json)
                {
                    valorTipado = json.ValueKind switch
                    {
                        JsonValueKind.String => detectarStringTipo(json.GetString()),
                        JsonValueKind.Number => detectarNumericoTipo(json),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        JsonValueKind.Array => json.GetRawText(),
                        JsonValueKind.Object => json.GetRawText(),
                        JsonValueKind.Undefined => null,
                        _ => parametro.Value.ToString() ?? ""
                    };
                }
                else
                {
                    valorTipado = parametro.Value;
                }
                parametrosG[nombre] = valorTipado ?? DBNull.Value;

            }
            return parametrosG;
        }
        /// <summary>
        /// Detecta el tipo adecuado de un string para SQL
        /// </summary>
        /// <param name="valor"></param>
        /// <returns></returns>
        private object detectarStringTipo(string? valor)
        {
            if (string.IsNullOrEmpty(valor))
            {
                return valor ?? "";
            }
            if(DateTime.TryParse(valor, out DateTime dateTimeValue))
            {
                return dateTimeValue;
            }
            if(int.TryParse(valor, out int intValue))
            {
                return intValue;
            }
            if (long.TryParse(valor, out long longValue))
            {
                return longValue;
            }
            if (bool.TryParse(valor, out bool boolValue))
            {
                return boolValue;
            }
            if (decimal.TryParse(valor, out decimal decimalValue))
            {
                return decimalValue;
            }
            if(double.TryParse(valor, out double doubleValue))
            {
                return doubleValue;
            }
            if(Guid.TryParse(valor, out Guid guidValue))
            {
                return guidValue;
            }
            return valor;
        }
        /// <summary>
        /// Detecta el tipo adecuado de un número para SQL
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private object detectarNumericoTipo(JsonElement json)
        {
            if(json.TryGetInt32(out int intValue))
            {
                return intValue;
            }
            if (json.TryGetInt64(out long longValue))
            {
                return longValue;
            }
            if (json.TryGetDouble(out double decimalValue))
            {
                return decimalValue;
            }
            return json.ToString()?? "";
        }
        public Dictionary<string,object> ConvertirParametrosEncriptados(Dictionary<string,object?>? parametros, List<string>? camposEncriptar)
        {
            // contraseña 123456 -> $2a$12$KIXQJf6Z8Hf3b8j1y5E5EuJ8mFz5e5e5e5e5e5e5e5e5e5e5e5e
            var parametrosGenericos = ConvertirParametrosJson(parametros);
            foreach(var campo in camposEncriptar ?? new List<string>())
            {
                string nombreCampo = campo.StartsWith("@") ? campo : "@" + campo;
                if (parametrosGenericos.ContainsKey(nombreCampo) && parametrosGenericos[nombreCampo] is string valorString &&
                    !string.IsNullOrEmpty(valorString) && !valorString.StartsWith("$2"))
                {
                    parametrosGenericos[nombreCampo] = BCrypt.Net.BCrypt.HashPassword(valorString,workFactor:12);
                }
                else
                {
                    throw new ArgumentException($"El campo a encriptar '{nombreCampo}' no existe en los parámetros o no es un string válido.");
                }
            }
            return parametrosGenericos;
        }
    }
   
}
