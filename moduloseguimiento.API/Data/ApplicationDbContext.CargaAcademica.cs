using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        public List<Periodo> CatalogoPeriodos()
        {
            List<Periodo> Periodos = new List<Periodo>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_CatalogoPeriodos", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var p = new Periodo
                            {
                                cve_Periodo = reader["CVE_Periodo"].ToString(),
                                periodo = reader["DescPeriodo"]?.ToString()
                            };
                            Periodos.Add(p);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log o lanza excepción personalizada si es necesario
                    throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
                }
            }

            return Periodos;
        }

        public List<GetFacilitadoresXPeriodo> FacilitadoresXPeriodo(string monitorArea, string Periodo, out string salida, out int estado)
        {
            List<GetFacilitadoresXPeriodo> facilitadores = new List<GetFacilitadoresXPeriodo>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_Facilitadores_X_Periodo", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@MonitorArea", monitorArea));
                    command.Parameters.Add(new SqlParameter("@Periodo", Periodo));

                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estatus", SqlDbType.Int)
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
                                GetFacilitadoresXPeriodo facXPE = new GetFacilitadoresXPeriodo
                                {
                                    idUsuario = reader["IdUsuario"].ToString(),
                                    nombreFacilitador = reader["NombreFacilitador"].ToString(),
                                };
                                facilitadores.Add(facXPE);
                            }
                        }

                        // Ya puedes leer los parámetros de salida aquí sin ejecutar nada más
                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estatus"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estatus"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                }
            }

            return facilitadores;
        }


        public List<ProgramaEducativo> ProgramasEducativosXFacilitador(string Periodo, string idUsuarioDoc, out string salida, out int estado)
        {
            List<ProgramaEducativo> ProgramasEducativosXFacilitador = new List<ProgramaEducativo>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_ProgramasEducativos_X_Facilitador", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@Periodo", Periodo));
                    command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));

                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estatus", SqlDbType.Int)
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
                                ProgramaEducativo PEs = new ProgramaEducativo
                                {
                                    Cve_PE = reader["CVE_PROGRAMA_EDUCATIVO"].ToString(),
                                    programaEducativo = reader["DescProgramaEducativo"].ToString(),
                                };
                                ProgramasEducativosXFacilitador.Add(PEs);
                            }
                        }

                        // Ya puedes leer los parámetros de salida aquí sin ejecutar nada más
                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estatus"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estatus"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                }
            }

            return ProgramasEducativosXFacilitador;
        }

        public List<EEPorPEPeriodoDocente> ExperienciaEducativasXPE_Periodo_Docente(string idUsuarioDoc, string Cve_Pe, string Periodo, out string salida, out int estado)
        {
            List<EEPorPEPeriodoDocente> ExperienciasEducativas = new List<EEPorPEPeriodoDocente>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_EE_X_PE_Periodo_Docente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));
                    command.Parameters.Add(new SqlParameter("@CvePe", Cve_Pe));
                    command.Parameters.Add(new SqlParameter("@Periodo", Periodo));

                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estatus", SqlDbType.Int)
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
                                EEPorPEPeriodoDocente EE = new EEPorPEPeriodoDocente
                                {
                                    id_infoAcademicoPE_EE = int.Parse(reader["Id_InfoAcademicoPE_EE"].ToString()),
                                    cve_EE = reader["CVE_EE"].ToString(),
                                    ExperienciaEducativa = reader["DESC_EE"].ToString(),
                                };
                                ExperienciasEducativas.Add(EE);
                            }
                        }

                        // Ya puedes leer los parámetros de salida aquí sin ejecutar nada más
                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estatus"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estatus"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                }
            }

            return ExperienciasEducativas;
        }


        public List<GetHorarioEE> HorarioExperienciaEducativa(string ExperienciaEducativa, string Cve_EE, string Cve_Pe, string Periodo, string idUsuarioDoc, out string salida, out int estado)
        {
            List<GetHorarioEE> HorarioEE = new List<GetHorarioEE>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_HorarioEE_X_PE_Periodo_Docente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@experienciaEducativa", ExperienciaEducativa));
                    command.Parameters.Add(new SqlParameter("@CveEE", Cve_EE));
                    command.Parameters.Add(new SqlParameter("@CvePe", Cve_Pe));
                    command.Parameters.Add(new SqlParameter("@Periodo", Periodo));
                    command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));

                    SqlParameter salidaParam = new SqlParameter("@salida", SqlDbType.NVarChar, -1)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(salidaParam);

                    SqlParameter estadoParam = new SqlParameter("@estatus", SqlDbType.Int)
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
                                GetHorarioEE horario = new GetHorarioEE
                                {
                                    id_InfoAcademicoPE_EE = int.Parse(reader["Id_InfoAcademicoPE_EE"].ToString()),
                                    idUsuarioDoc = reader["pk_Usuario"].ToString(),
                                    cve_Periodo = reader["CVE_PERIODO"].ToString(),
                                    cve_programaEducativa = reader["CVE_PROGRAMA_EDUCATIVO"].ToString(),
                                    cve_ExperienciaEducativa = reader["CVE_EE"].ToString(),
                                    lunes = reader["LUNES"].ToString(),
                                    martes = reader["MARTES"].ToString(),
                                    miercoles = reader["MIERCOLES"].ToString(),
                                    jueves = reader["JUEVES"].ToString(),
                                    viernes = reader["VIERNES"].ToString(),
                                    sabado = reader["SABADO"].ToString(),
                                    domingo = reader["DOMINGO"].ToString(),
                                    totalHorasImparte = reader["HORAS_TOTALES_IMPARTE"].ToString(),
                                };
                                HorarioEE.Add(horario);
                            }
                        }

                        // Ya puedes leer los parámetros de salida aquí sin ejecutar nada más
                        salida = command.Parameters["@salida"].Value != DBNull.Value ? command.Parameters["@salida"].Value.ToString() : string.Empty;
                        estado = command.Parameters["@estatus"].Value != DBNull.Value ? Convert.ToInt32(command.Parameters["@estatus"].Value) : 0;
                    }
                    catch (SqlException ex)
                    {
                        salida = ex.Number + " - " + ex.Message;
                        estado = -1;
                    }
                }
            }

            return HorarioEE;
        }


        public Paginacion<IncidenciaAcceso> IncidenciasAcceso_X_Periodo_Facilitador(
        string Periodo,
        string idUsuarioDoc,
        int pageNumber,
        int pageSize,
        DateTime? fechaInicio,
        DateTime? fechaFin,
        out string salida,
        out int estado)
        {
            var result = new Paginacion<IncidenciaAcceso>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<IncidenciaAcceso> todasIncidencias = new List<IncidenciaAcceso>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_IncidenciasAcceso_X_Periodo_Facilitador", connection))
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
                            todasIncidencias.Add(new IncidenciaAcceso
                            {
                                fechaIncidencia = Convert.ToDateTime(reader["FechaIncidencia"]).ToString("dd/MM/yyyy"),
                                horaProgramadaEE = reader["HoraProgramada"].ToString(),
                                cve_EEProgramada = reader["CVE_EE_Programada"].ToString(),
                                experienciaEducativaProgramada = reader["ExperienciaEducativaProgramada"].ToString(),
                                cve_ProgramaEducativo = reader["CVE_ProgramaEducativo"].ToString(),
                                programaEducativo = reader["DescProgramaEducativo"].ToString(),
                                cve_EEAccedida = reader["CVE_EE_Accedida"].ToString(),
                                experienciaEducativaAccedida = reader["ExperienciaEducativaAccedida"].ToString(),
                                //FechaHoraAcceso = reader["FechaHoraAcceso"].ToString(),
                                FechaHoraAcceso = Convert.ToDateTime(reader["FechaHoraAcceso"])
                                 .ToString("yyyy-MM-dd HH:mm:ss"),
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

            IEnumerable<IncidenciaAcceso> query = todasIncidencias;

            // ✅ Filtro SOLO por rango personalizado
            if (fechaInicio.HasValue && fechaFin.HasValue)
            {
                var inicio = fechaInicio.Value.Date;
                var fin = fechaFin.Value.Date;

                query = query.Where(i =>
                    DateTime.TryParseExact(i.fechaIncidencia, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime f) &&
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
