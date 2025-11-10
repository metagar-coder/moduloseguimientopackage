using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        
        //Lista de monitores con Filtro
        public Paginacion<MonitorPE> ObtenerMonitoresPEFiltrado(string monitorArea, int pageNumber, int pageSize, string? busquedaGeneral, string? rol, string? dependencia,
            string? region, out string salida, out int estado)
            {
            var result = new Paginacion<MonitorPE>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<MonitorPE> monitorPEs = new List<MonitorPE>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_MonitoresPE", connection))
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
                            monitorPEs.Add(new MonitorPE
                            {
                                usuario = reader["Usuario"].ToString(),
                                nombre = reader["Nombre"].ToString(),
                                correoInstitucional = reader["CorreoInstitucional"].ToString(),
                                rol = reader["Rol"].ToString(),
                                idTipoPerfil = Convert.ToInt32(reader["IdTipoPerfil"].ToString()),
                                nombreMonitorArea = reader["NombreMonitorArea"].ToString(),
                                idPEDependencia = Convert.ToInt32(reader["IdPEDependencia"].ToString()),
                                claveProgramaEducativo = reader["ClaveProgramaEducativo"].ToString(),
                                programaEducativo = reader["DescProgramaEducativo"].ToString(),
                                Cve_dependencia = int.Parse(reader["Cve_Dependencia"].ToString()),
                                dependencia = reader["Dependencia"].ToString(),
                                region = reader["Region"].ToString(),
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

            // Aplicar filtros si vienen
            var query = monitorPEs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerbusquedaGeneral = busquedaGeneral.ToLower();
                query = query.Where(m =>
                    (m.nombre ?? "").ToLower().Contains(lowerbusquedaGeneral) ||
                    (m.correoInstitucional ?? "").ToLower().Contains(lowerbusquedaGeneral) ||
                    (m.rol ?? "").ToLower().Contains(lowerbusquedaGeneral) ||
                    (m.programaEducativo ?? "").ToLower().Contains(lowerbusquedaGeneral) ||
                    (m.dependencia ?? "").ToLower().Contains(lowerbusquedaGeneral) ||
                    (m.region ?? "").ToLower().Contains(lowerbusquedaGeneral));
            }
            if (!string.IsNullOrWhiteSpace(rol))
                query = query.Where(m => m.rol.Equals(rol, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(dependencia))
                query = query.Where(m => m.dependencia.Equals(dependencia, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(region))
                query = query.Where(m => m.region.Equals(region, StringComparison.OrdinalIgnoreCase));

            // Paginación
            result.TotalRegistros = query.Count();
            result.TotalPaginas = (int)Math.Ceiling(result.TotalRegistros / (double)pageSize);
            result.Items = query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return result;
        }


        // Catalogo de Dependencias
        public List<Dependencia> ListaDependencias(string monitorArea, out string salida, out int estado)
        {
            List<Dependencia> dependencias = new List<Dependencia>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_EMSCDependencia", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@MonitorArea", monitorArea));

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
                                Dependencia dependencia = new Dependencia
                                {
                                    Cve_Dependencia = int.Parse(reader["Cve_Dependencia"].ToString()),
                                    dependencia = reader["Dependencia"].ToString(),
                                };
                                dependencias.Add(dependencia);
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

            return dependencias;
        }


        // Catalogo de Programa Educativos X Dependencia
        public List<ProgramaEducativo> ListaPE_X_Dependencias(string dependencia, string monitorArea, out string salida, out int estado)
        {
            List<ProgramaEducativo> ProgramasEducativos = new List<ProgramaEducativo>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_DependenciaPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@pr_Dependencia", dependencia));
                    command.Parameters.Add(new SqlParameter("@MonitorArea", monitorArea));

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
                                ProgramaEducativo PE = new ProgramaEducativo
                                {
                                    Cve_PE = reader["Cve_ProgramaEducativo"].ToString(),
                                    programaEducativo = reader["ProgramaEducativo"].ToString(),
                                };
                                ProgramasEducativos.Add(PE);
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

            return ProgramasEducativos;
        }


        //Inactivar la relacion de un monitor PE con un programa Educativo (Es para el boton de eliminar)
        public void BajaMonitorPE(string usuario, int idPEDependencia, out string salida, out int estado)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPE_MonitorPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada
                    command.Parameters.Add(new SqlParameter("@pr_Usuario", usuario));
                    command.Parameters.Add(new SqlParameter("@pr_IdPEDependencia", idPEDependencia));

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


        //Actualizar MonitorPE
        //Registrar una nueva relacion de un MonitorPE a un Programa educativo (Activo) y desactivar la relacion actual (Inactivo)
        public void actualizarMonitorPE(string usuario, string dependenciaNew, string programaEducativoNew, int idPEDependenciaActual, out string salida, out int estado)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPA_MonitorPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetro de entrada
                    command.Parameters.Add(new SqlParameter("@pr_Usuario", usuario));
                    command.Parameters.Add(new SqlParameter("@pr_Dependencia", dependenciaNew));
                    command.Parameters.Add(new SqlParameter("@pr_ProgramaEducativo", programaEducativoNew));
                    command.Parameters.Add(new SqlParameter("@pr_IdPEDependenciaAnt", idPEDependenciaActual));

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

        // Registrar un nuevo MonitorPE (Boton de Agregar)
        public void RegistrarMonitorPE(NewMonitorPE monitorPE, out string salida, out int estado)
        {
            salida = string.Empty;
            estado=0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPI_AgregarMonitorPE", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@pr_Usuario", monitorPE.usuario));
                    command.Parameters.Add(new SqlParameter("@pr_Nombre", monitorPE.nombre));
                    command.Parameters.Add(new SqlParameter("@pr_ApPaterno", monitorPE.apellidoPat));
                    command.Parameters.Add(new SqlParameter("@pr_ApMaterno", monitorPE.apellidoMat));
                    command.Parameters.Add(new SqlParameter("@pr_CorreoInstitucional", monitorPE.correoInstitucional));
                    command.Parameters.Add(new SqlParameter("@pr_CorreoAlterno", monitorPE.correoAlterno));
                    command.Parameters.Add(new SqlParameter("@pr_TelContacto", monitorPE.telContacto));
                    command.Parameters.Add(new SqlParameter("@pr_NumPersonal", monitorPE.numPersonal.ToString()));
                    command.Parameters.Add(new SqlParameter("@pr_Dependencia", monitorPE.dependencia));
                    command.Parameters.Add(new SqlParameter("@pr_ProgramaEducativo", monitorPE.programaEducativo));

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


        //*********************************************************************************************************************************

        //Lista de Experiencias Educativas por facilitador
        public List<GetListaEE_X_facilitador> ListaEE_X_Facilitador(string monitorDoc, string Cve_PE, string Cve_Dependencia, out string salida, out int estado)
        {
            List<GetListaEE_X_facilitador> listaExperienciasEducativas = new List<GetListaEE_X_facilitador>();
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPS_CursosDocente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@pr_IdDocente", monitorDoc));
                    command.Parameters.Add(new SqlParameter("@pr_PE", Cve_PE));
                    command.Parameters.Add(new SqlParameter("@pr_idDependencia", Cve_Dependencia));

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
                                GetListaEE_X_facilitador listaEE = new GetListaEE_X_facilitador
                                {
                                    idUsuarioCurso = int.Parse(reader["IdUsuarioCurso"].ToString()),
                                    idCurso = int.Parse(reader["IdCurso"].ToString()),
                                    idUsuario = reader["IdUsuario"].ToString(),
                                    nombreCurso = reader["Nombre"].ToString(),
                                    descripcionCurso = reader["Descripcion"].ToString(),
                                    cve_Dependencia = reader["IdDependencia"].ToString(),
                                    cve_ProgramaEducativo = reader["IdPrograma"].ToString(),
                                    nombreDocente = reader["NombreDocente"].ToString(),
                                    programaEducativo = reader["ProgramaEducativo"].ToString(),
                                    dependencia = reader["Dependencia"].ToString()
                                };
                                listaExperienciasEducativas.Add(listaEE);
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

            return listaExperienciasEducativas;
        }


        //Relacionar el MonitorPE-Programa educativo con un facilitador y su experiencia educativa
        public void RelacionMonitorPE_EE(NewMonitorPE_EE monitorPE_EE, out string salida, out int estado)
        {
            salida = string.Empty;
            estado = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand command = new SqlCommand("SPIA_AgregarMonitorDocente", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.Add(new SqlParameter("@pr_IdPEDep", monitorPE_EE.idPEDependencia));
                    command.Parameters.Add(new SqlParameter("@pr_IdUsuarioMon", monitorPE_EE.usuarioPE));
                    command.Parameters.Add(new SqlParameter("@pr_IdUsuarioDoc", monitorPE_EE.usuarioDoc));
                    command.Parameters.Add(new SqlParameter("@pr_IdCurso", monitorPE_EE.idCurso));

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

    }
}
