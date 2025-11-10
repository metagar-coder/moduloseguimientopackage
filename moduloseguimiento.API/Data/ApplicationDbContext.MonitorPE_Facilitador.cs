using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        // Lista de MonitoresPE con sus respectivos facilitadores y Experiencias Educativas a su cargo.
        public Paginacion<GetMonitorPE_FacilitadorEE> ObtenerMonitoresPE_FacEE_Filtrado(
        string monitorArea, int pageNumber, int pageSize,
        string? busquedaGeneral, string? PE, string? region,
        out string salida, out int estado)
        {
            var result = new Paginacion<GetMonitorPE_FacilitadorEE>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            salida = string.Empty;
            estado = 0;

            // Diccionario para agrupar
            var agrupados = new Dictionary<string, GetMonitorPE_FacilitadorEE>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_MonitoresFacilitadores", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("@MonitorArea", monitorArea));
                command.Parameters.Add(new SqlParameter("@salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output });
                command.Parameters.Add(new SqlParameter("@estado", SqlDbType.Int) { Direction = ParameterDirection.Output });

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var monitor = reader["MonitorPE"].ToString();
                            var programa = reader["Cve_PE"].ToString();
                            var key = monitor + "|" + programa;

                            if (!agrupados.ContainsKey(key))
                            {
                                agrupados[key] = new GetMonitorPE_FacilitadorEE
                                {
                                    usuarioPE = reader["UsuarioPE"]?.ToString(),
                                    idTipoPerfil = reader["fk_IdTipoPerfil"] != DBNull.Value ? Convert.ToInt32(reader["fk_IdTipoPerfil"]) : 0,
                                    monitorPE = monitor,
                                    usuarioArea = reader["UsuarioArea"]?.ToString(),
                                    monitorArea = reader["MonitorArea"]?.ToString(),
                                    idPEDependencia = reader["fk_IdPEDependencia"] != DBNull.Value ? Convert.ToInt32(reader["fk_IdPEDependencia"]) : 0,
                                    cve_Dependencia = reader["Cve_Dependencia"]?.ToString(),
                                    dependencia = reader["Dependencia"]?.ToString(),
                                    cve_PE = programa,
                                    programaEducativo = reader["ProgramaEducativo"]?.ToString(),
                                    region = reader["Region"]?.ToString(),
                                    facilitadoresEE = new List<Facilitador_EE>()
                                };
                            }

                            agrupados[key].facilitadoresEE.Add(new Facilitador_EE
                            {
                                facilitador = reader["Facilitador"].ToString(),
                                experienciaEducativa = reader["EE"].ToString()
                            });
                        }
                    }

                    salida = command.Parameters["@salida"].Value?.ToString() ?? string.Empty;
                    estado = Convert.ToInt32(command.Parameters["@estado"].Value ?? 0);
                }
                catch (SqlException ex)
                {
                    salida = ex.Number + " - " + ex.Message;
                    estado = -1;
                    return result;
                }
            }

            // Convertimos a lista
            var lista = agrupados.Values.ToList();

            // Filtros
            var query = lista.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerBusqueda = busquedaGeneral.ToLower();
                query = query.Where(m =>
                    (m.monitorPE ?? "").ToLower().Contains(lowerBusqueda) ||
                    (m.programaEducativo ?? "").ToLower().Contains(lowerBusqueda) ||
                    (m.cve_PE ?? "").ToLower().Contains(lowerBusqueda) ||
                    m.facilitadoresEE.Any(f =>
                        (f.facilitador ?? "").ToLower().Contains(lowerBusqueda) ||
                        (f.experienciaEducativa ?? "").ToLower().Contains(lowerBusqueda)));
            }

            if (!string.IsNullOrWhiteSpace(PE))
            {
                var peLower = PE.ToLower();
                query = query.Where(m =>
                    (!string.IsNullOrEmpty(m.programaEducativo) && m.programaEducativo.ToLower().Contains(peLower)) ||
                    (!string.IsNullOrEmpty(m.cve_PE) && m.cve_PE.ToLower().Contains(peLower))
                );
            }


            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(m => m.region != null &&
                    m.region.Contains(region, StringComparison.OrdinalIgnoreCase));

            // Paginación
            result.TotalRegistros = query.Count();
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            result.Items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return result;
        }


        public List<GetFacilitadoresEE> ObtenerFacEE_X_MonitorPE(string usuarioPE, string usuarioArea, string Cve_PE, out string salida, out int estado)
        {
            List<GetFacilitadoresEE> facilitadoresEE = new List<GetFacilitadoresEE>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_DatosFacilitadores", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@MonitorPE", usuarioPE));
                    command.Parameters.Add(new SqlParameter("@MonitorArea", usuarioArea));
                    command.Parameters.Add(new SqlParameter("@ProgramaEducativo", Cve_PE));

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
                                GetFacilitadoresEE facEE = new GetFacilitadoresEE
                                {
                                    idMonitorDoc = int.Parse(reader["IdMonitorDoc"].ToString()),
                                    nombreDocente = reader["NombreDocente"].ToString(),
                                    experienciaEducativa = reader["ExperienciaEducativa"].ToString(),
                                };
                                facilitadoresEE.Add(facEE);
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

            return facilitadoresEE;
        }


        public void BajaRelacionFacEE_MonitorPE(int idMonitorDoc, out string salida, out int estado)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPE_MonitorDocente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada
                    command.Parameters.Add(new SqlParameter("@pr_IdMonitorDoc", idMonitorDoc));

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


        public List<GetCatalogoPE_X_CS> CatalogoPE_X_CS(string monitorArea)
        {
            List<GetCatalogoPE_X_CS> programasEducativos = new List<GetCatalogoPE_X_CS>();

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
                            var pe = new GetCatalogoPE_X_CS
                            {
                                cve_PE = reader["fkpk_ProgramaEducativo"]?.ToString(),
                                programaEducativo = reader["DescProgramaEducativo"]?.ToString()
                            };
                            programasEducativos.Add(pe);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log o lanza excepción personalizada si es necesario
                    throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
                }
            }

            return programasEducativos;
        }


        public List<AreaAcademica> CatalogoAreaAcademica()
        {
            List<AreaAcademica> AreasAcademicas = new List<AreaAcademica>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_CatalogoAreasAcademicas", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var aa = new AreaAcademica
                            {
                                idArea = int.Parse(reader["pk_AreaAcademica"].ToString()),
                                areaAcademica = reader["DescAreaAcademica"]?.ToString()
                            };
                            AreasAcademicas.Add(aa);
                        }
                    }
                }
                catch (SqlException ex)
                {
                    // Log o lanza excepción personalizada si es necesario
                    throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
                }
            }

            return AreasAcademicas;
        }

    }
}
