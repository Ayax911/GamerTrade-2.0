using System.Data;

namespace APiGamer.Repositorio.Abstracciones
{
   
    public interface IConexionFactory
    {
        
        IDbConnection CrearConexion();
        
       
        string TipoBaseDatos { get; }
    }
}