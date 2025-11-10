using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;
using System.Threading;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {
        public void RegistrarConfiguracionPE(ConfiguracionProgramaEducativo configuracionPE, out string salida, out int estado)
        {
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPIA_ConfiguracionesPE", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@pr_ProgramaEducativo", configuracionPE.programaEducativo));
                    command.Parameters.Add(new SqlParameter("@pr_Dependencia", configuracionPE.dependencia));
                    command.Parameters.Add(new SqlParameter("@pr_IdTipoCal", configuracionPE.idTipoCalendario));
                    command.Parameters.Add(new SqlParameter("@pr_HoraMaxAusenciaDoc", configuracionPE.horaMaxAusenciaFacilitador));
                    command.Parameters.Add(new SqlParameter("@pr_MinMaxAusenciaDoc", configuracionPE.minMaxAusenciaFacilitador));
                    command.Parameters.Add(new SqlParameter("@pr_HoraMaxRevisionAct", configuracionPE.horaMaxRevisionAct_Fac));
                    command.Parameters.Add(new SqlParameter("@pr_MinMaxRevisionAct", configuracionPE.minMaxRevisionAct_Fac));
                    command.Parameters.Add(new SqlParameter("@pr_HoraMaxForos", configuracionPE.horaMaxForoFacilitador));
                    command.Parameters.Add(new SqlParameter("@pr_MinMaxForos", configuracionPE.minMaxForosFacilitador));
                    command.Parameters.Add(new SqlParameter("@pr_HoraMaxAusenciaEst", configuracionPE.horaMaxAusenciaEstudiante));
                    command.Parameters.Add(new SqlParameter("@pr_MinMaxAusenciaEst", configuracionPE.minMaxAusenciaEstudiante));
                    command.Parameters.Add(new SqlParameter("@pr_ActividadesSinEnt", configuracionPE.ActividadesSinEntregarEst));
                    command.Parameters.Add(new SqlParameter("@pr_ExamenesRep", configuracionPE.ExamenesReprobadorEst));
                    command.Parameters.Add(new SqlParameter("@pr_ForosSinPart", configuracionPE.ForosSinPartiEst));
                    command.Parameters.Add(new SqlParameter("@Periodo", configuracionPE.Periodo));

                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1);
                    salidaParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estado", SqlDbType.Int);
                    estadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(estadoParam);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();

                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estado"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estado"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                    finally
                    {
                        connection.Close();
                    }

                }
            }

        }


        public List<GetConfiguracionPE> ListaConfiguracionesPE(string usuarioArea, string? cve_PE, string? cve_Dependencia, string? periodo, out string salida, out int estado)
        {
            List<GetConfiguracionPE> configuraciones = new List<GetConfiguracionPE>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_ConfiguracionesPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@MonitorArea", usuarioArea));
                    command.Parameters.AddWithValue("@CvePE", (object?)cve_PE ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CveDependencia", (object?)cve_Dependencia ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Periodo", (object?)periodo ?? DBNull.Value);


                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estado", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(estadoParam);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                GetConfiguracionPE configuracion = new GetConfiguracionPE
                                {
                                    IdPEConf = int.Parse(reader["idPEConf"].ToString()),
                                    Cve_ProgramaEducativo = reader["fkpk_ProgramaEducativo"].ToString(),
                                    ProgramaEducativo = reader["DescProgramaEducativo"].ToString(),
                                    Cve_Dependencia = reader["fkpk_Dependencia"].ToString(),
                                    Dependencia = reader["DescDependencia"].ToString(),
                                    IdTipoCalendario = int.Parse(reader["fk_idTipoCal"].ToString()),
                                    Calendario = reader["DescTipoCalendario"].ToString(),
                                    Cve_Periodo = reader["CVE_Periodo"].ToString(),
                                    Periodo = reader["Periodo"].ToString(),
                                    horaMaxAusenciaFacilitador = int.Parse(reader["HoraMaxAusenciaDoc"].ToString()),
                                    minMaxAusenciaFacilitador = int.Parse(reader["MinMaxAusenciaDoc"].ToString()),
                                    horaMaxRevisionAct_Fac = int.Parse(reader["HoraMaxRevisionAct"].ToString()),
                                    minMaxRevisionAct_Fac = int.Parse(reader["MinMaxRevisionAct"].ToString()),
                                    horaMaxForoFacilitador = int.Parse(reader["HoraMaxForos"].ToString()),
                                    minMaxForosFacilitador = int.Parse(reader["MinMaxForos"].ToString()),
                                    horaMaxAusenciaEstudiante = int.Parse(reader["HoraMaxAusenciaEst"].ToString()),
                                    minMaxAusenciaEstudiante = int.Parse(reader["MinMaxAusenciaEst"].ToString()),
                                    ActividadesSinEntregarEst  = int.Parse(reader["ActividadesSinEnt"].ToString()),
                                    ExamenesReprobadorEst = int.Parse(reader["ExamenesRep"].ToString()),
                                    ForosSinPartiEst = int.Parse(reader["ForosSinPart"].ToString()),
                                    UltimaActualizacion = reader["ultimaAct"].ToString(),
                                };
                                configuraciones.Add(configuracion);
                            }
                        }

                        // Ya puedes leer los parámetros de salida aquí sin ejecutar nada más
                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estado"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estado"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                }
            }

            return configuraciones;
        }


        public void EliminarConfiguracionPE(int idPEConf, out string salida, out int estado)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPE_ConfiguracionesPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada
                    command.Parameters.Add(new SqlParameter("@idPEConf", idPEConf));

                    // Parámetros de salida
                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1);
                    salidaParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estado", SqlDbType.Int);
                    estadoParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(estadoParam);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();

                        // Obtener valores de salida
                        salida = command.Parameters["@salida"].Value.ToString();
                        estado = Convert.ToInt32(command.Parameters["@estado"].Value);
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                    finally
                    {
                        connection.Close();
                    }
                }
            }
        }


        //Metodo para recuperar todos los programas educativos que tiene a su cargo un coordinador de seguimiento con paginación.
        public Paginacion<GetCatalogoPExCSConPaginacion> CatalogoPExCSConPaginacion(
        string monitorArea, int pageNumber, int pageSize, string? busquedaGeneral)
        {
            var result = new Paginacion<GetCatalogoPExCSConPaginacion>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<GetCatalogoPExCSConPaginacion> programasEducativos = new List<GetCatalogoPExCSConPaginacion>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_CatalogoPE_X_CoordinadorSeguimiento", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@IdMonitorArea", monitorArea));

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var pe = new GetCatalogoPExCSConPaginacion
                            {
                                idRegion = int.Parse(reader["IdRegion"]?.ToString()),
                                region = reader["Region"]?.ToString(),
                                cve_Dependencia = reader["fkpk_Dependencia"]?.ToString(),
                                dependencia = reader["DescDependencia"]?.ToString(),
                                cve_PE = reader["fkpk_ProgramaEducativo"]?.ToString(),
                                programaEducativo = reader["DescProgramaEducativo"]?.ToString()
                            };
                            programasEducativos.Add(pe);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
                }
            }

            // Aplicar filtro de búsqueda general si corresponde
            var query = programasEducativos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var busqueda = busquedaGeneral.ToLower();
                query = query.Where(pe =>
                    (pe.region ?? "").ToLower().Contains(busqueda) ||
                    (pe.cve_Dependencia ?? "").ToLower().Contains(busqueda) ||
                    (pe.dependencia ?? "").ToLower().Contains(busqueda) ||
                    (pe.cve_PE ?? "").ToLower().Contains(busqueda) ||
                    (pe.programaEducativo ?? "").ToLower().Contains(busqueda)
                );
            }

            // Calcular paginación
            result.TotalRegistros = query.Count();
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            result.Items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return result;
        }


    }
}
