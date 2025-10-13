using APiGamer.Repositorio.Abstracciones;
using APiGamer.Servicios.Abstracciones;
using Microsoft.Data.SqlClient;
using System.Data;
namespace APiGamer.Repositorio
{
    public class RepositorioConsulta:IRepositorioConsulta
    {
        readonly IProvedor _provedor;
        public RepositorioConsulta(IProvedor provedor)
        {
            _provedor = provedor ?? throw new ArgumentNullException(
               nameof(provedor),
               "IProvedor no puede ser null. Verificar configuración de inyección de dependencias en Program.cs."
           );
        }
        public Task<DataTable> EjecturaConsultaParametrizada(string consulta, Dictionary<string, object> parametros)
        {
            if (string.IsNullOrEmpty(consulta))
            {
                throw new ArgumentException("La consulta no puede ser nula o vacía.", nameof(consulta));
            }
            if (parametros == null)
            {
                throw new ArgumentNullException(nameof(parametros), "El diccionario de parámetros no puede ser null.");
            }
            var (Esvalida, mensaje) = ValidarConsulta(consulta, parametros).Result;
            if (!Esvalida)
            {
                throw new ArgumentException($"La consulta no es válida: {mensaje}", nameof(consulta));
            }
            try
            {
                _provedor.AbrirConexion();
                if (_provedor == null)
                {
                    throw new InvalidOperationException("El proveedor de base de datos no está inicializado.");
                }
                using (SqlConnection connection = _provedor.AbrirConexion())
                {
                    using (var command = _provedor.AbrirConexion().CreateCommand())
                    {
                        command.CommandText = consulta;
                        command.CommandType = CommandType.Text;
                        foreach (var parametro in parametros)
                        {
                            var sqlParameter = command.CreateParameter();
                            sqlParameter.ParameterName = parametro.Key;
                            sqlParameter.Value = parametro.Value ?? DBNull.Value;
                            command.Parameters.Add(sqlParameter);
                        }
                        using (var adapter = new SqlDataAdapter((SqlCommand)command))
                        {
                            var dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            _provedor.CerrarConexion();
                            return Task.FromResult(dataTable);
                           
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al ejecutar la consulta parametrizada.", ex);
            }
        }
        public Task<(bool Esvalida, string mensaje)> ValidarConsulta(string consulta, Dictionary<string, object> parametros)
        {
            throw new NotImplementedException();
        }
        public Task<DataTable> EjecturaProcedimientoAlmacenado(string consulta, Dictionary<string, object> parametros)
        {
            throw new NotImplementedException();
        }
    }
}
