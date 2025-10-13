using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace APiGamer.Servicios.Abstracciones
{
    public interface IProvedor
    {
        public string ObtenerCadenaDeConexion();

        public SqlConnection AbrirConexion();

        public void CerrarConexion();
      
    }
}
