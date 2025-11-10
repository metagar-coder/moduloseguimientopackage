using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        public async Task<(string salida, int estatusHTTP)> RegistrarCorreoEnviadoAsync(EmailEnviado emailEnviado)
        {
            string salida = string.Empty;
            int estatusHTTP = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPI_CorreoEnviado", connection))
            {
                command.CommandType = System.Data.CommandType.StoredProcedure;

                // Parámetros de entrada
                command.Parameters.Add(new SqlParameter("@Cve_PE", emailEnviado.cve_PE));
                command.Parameters.Add(new SqlParameter("@IdCurso", emailEnviado.idCurso));
                command.Parameters.Add(new SqlParameter("@Periodo", emailEnviado.periodo));
                command.Parameters.Add(new SqlParameter("@Asunto", emailEnviado.asunto));
                command.Parameters.Add(new SqlParameter("@Mensaje", emailEnviado.mensaje));
                command.Parameters.Add(new SqlParameter("@DestinatarioId", emailEnviado.destinatarioId));
                command.Parameters.Add(new SqlParameter("@RemitenteId", emailEnviado.remitenteId));

                // Parámetros de salida
                SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(salidaParam);

                SqlParameter estadoParam = new SqlParameter("@estatusHTTP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(estadoParam);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    // Obtener valores de salida
                    salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                    estatusHTTP = command.Parameters["@estatusHTTP"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estatusHTTP"].Value) : 0;
                }
                catch (SqlException ex)
                {
                    salida = ex.Number + " - " + ex.Message;
                    estatusHTTP = -1;
                }
            }

            return (salida, estatusHTTP);
        }

        public Paginacion<EmailsXFacilitador> CorreoEnviados_X_Periodo_Facilitador(
        string Periodo,
        string idUsuarioDoc,
        int pageNumber,
        int pageSize,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        out string salida,
        out int estado)
        {
            var result = new Paginacion<EmailsXFacilitador>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<EmailsXFacilitador> todosLosCorreos = new List<EmailsXFacilitador>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_CorreosEnviados_X_Periodo_Facilitador", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Periodo", Periodo);
                command.Parameters.AddWithValue("@IdUsuarioDoc", idUsuarioDoc);
                command.Parameters.Add(new SqlParameter("@salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@estatus", SqlDbType.Int) { Direction = ParameterDirection.Output });

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            todosLosCorreos.Add(new EmailsXFacilitador
                            {

                                idCorreoEnviado = int.Parse(reader["IdCorreoEnviado"].ToString()),
                                cve_PE = reader["Cve_PE"].ToString(),
                                programaEducativo = reader["DescProgramaEducativo"].ToString(),
                                idCurso = int.Parse(reader["IdCurso"].ToString()),
                                curso = reader["NombreCurso"].ToString(),
                                cve_Periodo = reader["Periodo"].ToString(),
                                periodo = reader["DescPeriodo"].ToString(),
                                asunto = reader["Asunto"].ToString(),
                                mensaje = reader["Mensaje"].ToString(),
                                destinatarioId = reader["DestinatarioId"].ToString(),
                                nombreDestinatario = reader["NombreDestinatario"].ToString(),
                                remitenteId = reader["RemitenteId"].ToString(),
                                nombreRemitente = reader["NombreRemitente"].ToString(),
                                fechaCorreoEnviado = reader["FechaCorreoEnviado"].ToString(),

                            });
                        }
                    }

                    salida = command.Parameters["@salida"].Value?.ToString() ?? string.Empty;
                    estado = Convert.ToInt32(command.Parameters["@estatus"].Value ?? 0);
                }
                catch (SqlException ex)
                {
                    salida = ex.Number + " - " + ex.Message;
                    estado = -1;
                    return result;
                }
            }

            IEnumerable<EmailsXFacilitador> query = todosLosCorreos;

            // ✅ Filtro SOLO por rango personalizado
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                var inicio = fechaInicio.Value.Date;
                var fin = fechaFin.Value.Date;

                query = query.Where(i =>
                    DateTime.TryParse(i.fechaCorreoEnviado, out DateTime f) &&
                    f.Date >= inicio && f.Date <= fin);
            }

            // Paginación
            result.TotalRegistros = query.Count();
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            result.Items = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return result;
        }

    }
}
