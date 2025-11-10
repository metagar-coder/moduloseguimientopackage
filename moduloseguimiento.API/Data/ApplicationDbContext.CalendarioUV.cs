using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        public void RegistrarDiasDescansoUV(List<CalendarioUV> listaDiasDescanso, out string salida, out int estado)
        {
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var diasDescanso in listaDiasDescanso)
                {
                    using (SqlCommand command = new SqlCommand("SPI_DiasDescansoCalendarioUV", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@IdCalendario", diasDescanso.IdCalendario));
                        command.Parameters.Add(new SqlParameter("@TipoCalendario", diasDescanso.TipoCalendario));
                        command.Parameters.Add(new SqlParameter("@IdTipoCal", diasDescanso.IdTipoCal_EMSCCalendarios));
                        command.Parameters.Add(new SqlParameter("@Calendario", diasDescanso.Calendario));
                        command.Parameters.Add(new SqlParameter("@DescripcionCalendario", diasDescanso.DescripcionCalendario));
                        command.Parameters.Add(new SqlParameter("@IdPeriodo", diasDescanso.IdPeriodo));
                        command.Parameters.Add(new SqlParameter("@CvePeriodo", (object?)diasDescanso.Cve_Periodo ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@TipoPeriodo", diasDescanso.TipoPeriodo));
                        command.Parameters.Add(new SqlParameter("@Periodo", diasDescanso.Periodo));
                        command.Parameters.Add(new SqlParameter("@IdFecha", (object?)diasDescanso.IdFecha ?? DBNull.Value));
                        command.Parameters.Add(new SqlParameter("@Fecha", diasDescanso.Fecha.ToString()));
                        command.Parameters.Add(new SqlParameter("@DiaSemana", diasDescanso.DiaSemana));
                        command.Parameters.Add(new SqlParameter("@Dia", diasDescanso.Dia));
                        command.Parameters.Add(new SqlParameter("@Mes", diasDescanso.Mes));
                        command.Parameters.Add(new SqlParameter("@Anio", diasDescanso.Anio));
                        command.Parameters.Add(new SqlParameter("@TipoDia", diasDescanso.TipoDia));
                        command.Parameters.Add(new SqlParameter("@DescripcionTipoDia", diasDescanso.DescripcionTipoDia));

                        SqlParameter salidaParam = new SqlParameter("@Salida", SqlDbType.NVarChar, -1)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(salidaParam);

                        SqlParameter estadoParam = new SqlParameter("@Estado", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(estadoParam);

                        try
                        {
                            command.ExecuteNonQuery();

                            salida = command.Parameters["@Salida"].Value != DBNull.Value
                                ? command.Parameters["@Salida"].Value.ToString()
                                : string.Empty;

                            estado = command.Parameters["@Estado"].Value != DBNull.Value
                                ? Convert.ToInt32(command.Parameters["@Estado"].Value)
                                : 0;
                        }
                        catch (SqlException ex)
                        {
                            salida = ex.Number + " - " + ex.Message;
                            estado = -1;
                            // Opcional: puedes decidir si quieres continuar con los demás o detener aquí
                            // break;
                        }
                    }
                }

                connection.Close();
            }
        }


        //Metodo para recuperar los dias de descanso del calendarioUV de un periodo especifico.
        public async Task<List<GetCalendarioUV>> DiasDescansoCalendarioUV_X_PeriodoAsync(string cvePeriodo)
        {
            List<GetCalendarioUV> DiasDescansoUV = new List<GetCalendarioUV>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_DiasDescanso_X_Periodo", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@cve_Periodo", cvePeriodo));

                try
                {
                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var dias = new GetCalendarioUV
                            {
                                IdCalendario = int.Parse(reader["IdCalendario"].ToString()),
                                TipoCalendario = int.Parse(reader["TipoCalendario"].ToString()),
                                Calendario = reader["Calendario"].ToString(),
                                DescripcionCalendario = reader["DescripcionCalendario"].ToString(),

                                IdPeriodo = int.Parse(reader["IdPeriodo"].ToString()),
                                Cve_Periodo = reader["Cve_Periodo"].ToString(),
                                TipoPeriodo = int.Parse(reader["TipoPeriodo"].ToString()),
                                Periodo = reader["Periodo"].ToString(),

                                IdFecha = reader["IdFecha"] != DBNull.Value ? int.Parse(reader["IdFecha"].ToString()) : (int?)null,
                                Fecha = reader["Fecha"].ToString(),
                                DiaSemana = reader["DiaSemana"].ToString(),
                                Dia = int.Parse(reader["Dia"]?.ToString()),
                                Mes = int.Parse(reader["Mes"]?.ToString()),
                                Anio = int.Parse(reader["Anio"]?.ToString()),
                                TipoDia = reader["TipoDia"].ToString(),
                                DescripcionTipoDia = reader["DescripcionTipoDia"].ToString(),
                            };

                            DiasDescansoUV.Add(dias);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
                }
            }

            return DiasDescansoUV;
        }

    }
}
