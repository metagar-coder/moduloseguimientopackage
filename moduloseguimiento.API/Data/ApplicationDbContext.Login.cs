using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Data;
using UAParser;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
            : this(configuration) // llama al otro constructor
        {
            _httpContextAccessor = httpContextAccessor;
        }

        //******************************* InicioSesionController *****************************************************

        // Método para verificar la existencia de un usuario utilizando el procedimiento almacenado
        public DataTable VerificarUsuario(string usuario)
        {

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_ValidaUsuario", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@IdUsuario", usuario));

                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    DataTable result = new DataTable();
                    adapter.Fill(result);

                    return result;
                }
            }
        }


        public int SetAccesoSistema(string idUsuario, string ip)
        {
            int idAccesoSistema = 0;

            // Obtener el UserAgent del request actual
            var userAgent = _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
            var parser = Parser.GetDefault();
            ClientInfo clientInfo = parser.Parse(userAgent);

            string navegador = clientInfo.UA.Family + " " + clientInfo.UA.Major;
            string sistema = clientInfo.OS.Family + " " + clientInfo.OS.Major;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPI_BTAccesosSistema", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@idUsuario", idUsuario));
                    command.Parameters.Add(new SqlParameter("@navegador", navegador));
                    command.Parameters.Add(new SqlParameter("@sistema", sistema));
                    command.Parameters.Add(new SqlParameter("@origenIP", ip));

                    var salida = new SqlParameter
                    {
                        ParameterName = "@salida",
                        SqlDbType = SqlDbType.NVarChar,
                        Size = 1000,
                        Direction = ParameterDirection.Output
                    };

                    var estado = new SqlParameter
                    {
                        ParameterName = "@estado",
                        SqlDbType = SqlDbType.Int,
                        Direction = ParameterDirection.Output
                    };

                    command.Parameters.Add(salida);
                    command.Parameters.Add(estado);

                    connection.Open();
                    command.ExecuteNonQuery();

                    int.TryParse(salida.Value?.ToString(), out idAccesoSistema);
                }
            }

            return idAccesoSistema;
        }

    }
}
