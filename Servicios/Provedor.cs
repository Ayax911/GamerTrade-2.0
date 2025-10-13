using APiGamer.Servicios.Abstracciones;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace APiGamer.Servicios
{
    public class Provedor:IProvedor
    {
        private readonly IConfiguration configuration;
        public Provedor(IConfiguration configuration)
        {

            this.configuration = configuration ?? throw new ArgumentNullException(
               nameof(configuration),
               "IConfiguration no puede ser null. Verificar configuración de inyección de dependencias en Program.cs."
           );

        }
        public string ObtenerCadenaDeConexion()
        {
            return configuration.GetConnectionString("ConnectionStrings") ?? "";
        }
        public SqlConnection AbrirConexion()
        {
            if (string.IsNullOrEmpty(ObtenerCadenaDeConexion()))
            {
                throw new InvalidOperationException("La cadena de conexión no puede ser nula o vacía. Verificar configuración en appsettings.json.");
            }
            try
            {
                SqlConnection connection = new SqlConnection(ObtenerCadenaDeConexion());
                connection.Open();
                return connection;
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al abrir la conexión a la base de datos.", ex);
            }
        }
        public void CerrarConexion()
        {
            if (string.IsNullOrEmpty(ObtenerCadenaDeConexion()))
            {
                throw new InvalidOperationException("La cadena de conexión no puede ser nula o vacía. Verificar configuración en appsettings.json.");
            }
            SqlConnection connection = new SqlConnection(ObtenerCadenaDeConexion());
            connection.Close();
        }
    }
}
