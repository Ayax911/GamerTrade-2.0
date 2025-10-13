using System.Data;

namespace APiGamer.Repositorio.Abstracciones
{
    public interface IRepositorioConsulta
    {
        Task<DataTable> EjecturaConsultaParametrizada(string consulta, Dictionary<string, object> parametros);

        Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string,object> parametros);

        Task<DataTable> EjecturaProcedimientoAlmacenado(string consulta,Dictionary<string,object> parametros);
    }
    
}
