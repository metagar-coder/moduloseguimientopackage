using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        public Paginacion<BitacoraAccesosEminus4> AccesosEminus4_X_Facilitador(
        string idUsuarioDoc,
        int pageNumber,
        int pageSize,
        DateTime? fechaInicio,
        DateTime? fechaFin)
        {
            var result = new Paginacion<BitacoraAccesosEminus4>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<BitacoraAccesosEminus4> accesos = new List<BitacoraAccesosEminus4>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_AccesosSistemaEminus4_X_Facilitador", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@IdUsuarioDoc", idUsuarioDoc);
                command.Parameters.AddWithValue("@FechaInicio", (object)fechaInicio ?? DBNull.Value);
                command.Parameters.AddWithValue("@FechaFin", (object)fechaFin ?? DBNull.Value);
                command.Parameters.AddWithValue("@PageNumber", pageNumber);
                command.Parameters.AddWithValue("@PageSize", pageSize);

                // Parámetro OUTPUT
                SqlParameter totalParam = new SqlParameter("@TotalRegistros", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(totalParam);

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            accesos.Add(new BitacoraAccesosEminus4
                            {
                                idUsuarioDoc = reader["IdUsuario"].ToString(),
                                fechaIngreso = reader["FechaIngreso"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["FechaIngreso"]).ToString(),
                                fechaSalida = reader["FechaSalida"] == DBNull.Value
                                    ? null
                                    : Convert.ToDateTime(reader["FechaSalida"]).ToString(),
                                tiempoPermanencia = reader["TiempoPermanencia"] == DBNull.Value
                                    ? null
                                    : reader["TiempoPermanencia"].ToString()
                            });
                        }
                    }

                    // Leer total de registros del parámetro OUTPUT
                    result.TotalRegistros = (int)totalParam.Value;
                    result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
                    result.Items = accesos;
                }
                catch (SqlException ex)
                {
                }
            }

            return result;
        }

    }
}
