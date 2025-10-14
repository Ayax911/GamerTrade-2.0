using System.Data;
namespace APiGamer.Servicios.Abstracciones
{
    public interface IServicioConsultas
    {
        Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object> parametros, string[] tablasProhibidas);
        Task<DataTable> EjecturaConsultaParametrizadaAsync(string consulta, Dictionary<string, object> parametros);
        Task<DataTable> EjecturaProcedimientoAlmacenadoAsync(string NombreSp, Dictionary<string, object> parametros);
        Task<DataTable> EjecutarConsultaParametrizadaDesdeJsonAsync(
          string consulta,
          Dictionary<string, object?>? parametros
      );

    }
}
