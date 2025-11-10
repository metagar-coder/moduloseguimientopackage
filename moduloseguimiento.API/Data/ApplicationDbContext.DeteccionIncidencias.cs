using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using moduloseguimiento.API.Models;
using System.Data;
using System.Globalization;
using System.Text.Json;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        #region Incidencias_de_Acceso
        // ********************************************************************************************************************************************************
        // ******************************* METODOS PARA LA DETENCCION DE INCIDENCIAS DE ACCESO (Choque de horarios de EE) ******************************************
        
        //++++++++ Metodo para recuperar los horarios de las experiencias educativas que imparte un facilitador. +++++++
        //ASINCRONO
        public async Task<List<GetHorariosXPeriodo>> HorariosXperiodoAsync(string anioPeriodo, string idUsuarioDoc)
        {
            List<GetHorariosXPeriodo> HorariosEE = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_HorariosXPeriodo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@AnioPeriodo", anioPeriodo));
            command.Parameters.Add(new SqlParameter("@idUsuarioDoc", idUsuarioDoc));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var horarios = new GetHorariosXPeriodo
                    {
                        id_infoAcademicoPE_EE = int.Parse(reader["Id_InfoAcademicoPE_EE"].ToString()),
                        idUsuarioDoc = reader["pk_Usuario"].ToString(),
                        cve_Periodo = reader["CVE_PERIODO"].ToString(),
                        periodo = reader["DESC_PERIODO"].ToString(),
                        cve_DependenciaEE = reader["CVE_DEP_EE"].ToString(),
                        cve_PE = reader["CVE_PROGRAMA_EDUCATIVO"].ToString(),
                        idPEDependencia = reader["fk_IDPEDEPENDENCIA"].ToString(),
                        cve_EE = reader["CVE_EE"].ToString(),
                        ExperienciaEducativa = reader["DESC_EE"].ToString(),
                        lunes = reader["LUNES"].ToString(),
                        martes = reader["MARTES"].ToString(),
                        miercoles = reader["MIERCOLES"].ToString(),
                        jueves = reader["JUEVES"].ToString(),
                        viernes = reader["VIERNES"].ToString(),
                        sabado = reader["SABADO"].ToString(),
                        domingo = reader["DOMINGO"].ToString(),
                        horasTotalesImparte = reader["HORAS_TOTALES_IMPARTE"].ToString(),
                    };
                    HorariosEE.Add(horarios);
                }
            }
            catch (SqlException)
            {
                // Puedes agregar manejo de errores
            }

            return HorariosEE;
        }

        // +++++++ Metodo para registrar las Incidencias detectadas. ++++++++
        //ASINCRONO
        public async Task<(string salida, int estado)> RegistrarIncidenciaAccesoLoteAsync(List<SetIncidenciasAccesoDetectadas> incidencias)
        {
            string salida = string.Empty;
            int estado = 200;

            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();
            using SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                foreach (var incidencia in incidencias)
                {
                    using SqlCommand command = new("SPI_IncidenciasAcceso_X_Doc", connection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    command.Parameters.AddWithValue("@fk_Usuario", (object?)incidencia.fk_Usuario ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CVE_ProgramaEducativo", (object?)incidencia.cve_ProgramaEducativo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CVE_Periodo", (object?)incidencia.cve_Periodo ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CVE_EE_Programada", (object?)incidencia.cve_EE_Programada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IdHorarioEE", incidencia.IdHorarioEE);
                    command.Parameters.AddWithValue("@FechaIncidencia", (object?)incidencia.FechaIncidencia ?? DBNull.Value);
                    command.Parameters.AddWithValue("@HoraProgramada", (object?)incidencia.HoraProgramada ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CVE_EE_Accedida", (object?)incidencia.cve_EE_Accedida ?? DBNull.Value);
                    command.Parameters.AddWithValue("@FechaHoraAcceso", (object?)incidencia.fechaHoraAcceso ?? DBNull.Value);

                    SqlParameter salidaParam = new("@Salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                    SqlParameter estadoParam = new("@Estado", SqlDbType.Int) { Direction = ParameterDirection.Output };

                    command.Parameters.Add(salidaParam);
                    command.Parameters.Add(estadoParam);

                    await command.ExecuteNonQueryAsync();

                    int estadoActual = estadoParam.Value != DBNull.Value ? Convert.ToInt32(estadoParam.Value) : 0;
                    string salidaActual = salidaParam.Value?.ToString() ?? "";

                    if (estadoActual != 200)
                    {
                        estado = estadoActual;
                        salida += $"Error con usuario {incidencia.fk_Usuario}: {salidaActual}\n";
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                estado = -1;
                salida = $"Excepción: {ex.Message}";
            }

            return (salida, estado);
        }


        //ASINCRONO
        public async Task<validacionIncidenciasAcceso> ValidarIncidenciasAccesoAsync(string cvePeriodo, string usuario)
        {
            var resultado = new validacionIncidenciasAcceso();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SPS_ValidarIncidenciasAccesoXFacilitador", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros
                cmd.Parameters.AddWithValue("@CVE_Periodo", cvePeriodo);
                cmd.Parameters.AddWithValue("@fk_Usuario", usuario);

                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        resultado.ExisteRegistro = reader.GetInt32(reader.GetOrdinal("ExisteRegistro"));

                        int fechaIndex = reader.GetOrdinal("UltimaFechaIncidencia");
                        if (!await reader.IsDBNullAsync(fechaIndex))
                        {
                            // GetDateTime funciona también con DATE
                            resultado.UltimaFechaIncidencia = reader.GetDateTime(fechaIndex);
                        }
                        else
                        {
                            resultado.UltimaFechaIncidencia = null;
                        }
                    }
                }
            }

            return resultado;
        }

        /*Este SP es para obtener los IdCursos que existen tanto en EMiNUS4 como en horarios de carga academica
	    esto con el fin de comparar los accesos de los cursos y ver si hay incidencias de acceso.*/
        //ASINCRONO
        public async Task<List<idCursos>> IdCursosXFacilitadorAsync(string idUsuario, string cvePeriodo)
        {
            var cursos = new List<idCursos>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SPS_IdCursosXFacilitador", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@CVE_Periodo", cvePeriodo);

                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cursos.Add(new idCursos
                        {
                            IdCurso = reader.GetInt32(reader.GetOrdinal("IdCurso")),
                            cve_curso = reader.IsDBNull(reader.GetOrdinal("CVE_EE"))
                                        ? null
                                        : reader.GetString(reader.GetOrdinal("CVE_EE"))
                        });
                    }
                }
            }

            return cursos;
        }

        //ASINCRONO
        public async Task<List<AccesosCursoEspecifico>> ObtenerAccesosCursoEspecificoAsync(string idUsuario, int idCurso)
        {
            var accesos = new List<AccesosCursoEspecifico>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand("SPS_AccesosCursoEspecifico", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                // Parámetros
                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                cmd.Parameters.AddWithValue("@IdCurso", idCurso);

                await conn.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        accesos.Add(new AccesosCursoEspecifico
                        {
                            IdCurso = reader.GetInt32(reader.GetOrdinal("IdCurso")),
                            IdUsuario = reader.GetString(reader.GetOrdinal("IdUsuario")),
                            FechaHora = reader.GetDateTime(reader.GetOrdinal("fechaHora")),
                        });
                    }
                }
            }

            return accesos;
        }

        // *******************************************************************************************************************************************************
        #endregion

        //*************************************************************************************

        #region Incidencias_de_Asistencia
        // ********************************************************************************************************************************************************
        // ******************************* METODOS PARA LA DETENCCION DE INCIDENCIAS DE ASISTENCIA DE UN FACILITADOR **********************************************

        // +++++++++++ METODO PARA SABER LOS ACCESOS DE UN FACILITADOR EN UN CURSO ESPECIFICIO +++++++++++++
        //ASINCRONO
        public async Task<(List<AccesosEminusXDocenteEE> accesos, string salida, int estado)> AccesosEminus_X_docenteEEAsync(int idCurso, string idUsuarioDoc)
        {
            List<AccesosEminusXDocenteEE> accesosXDocenteEE = new();
            string salida = string.Empty;
            int estado = 0;

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_UltimosAccesosEminus_X_docenteEE", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@IdCurso", idCurso));
            command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));

            SqlParameter salidaParam = new("@salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
            SqlParameter estadoParam = new("@estatus", SqlDbType.Int) { Direction = ParameterDirection.Output };

            command.Parameters.Add(salidaParam);
            command.Parameters.Add(estadoParam);

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    accesosXDocenteEE.Add(new AccesosEminusXDocenteEE
                    {
                        idUsuarioDoc = reader["IdUsuario"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        FechaHoraAcceso = reader["FechaHora"].ToString(),
                        cve_Programa = reader["IdPrograma"].ToString(),
                    });
                }

                salida = salidaParam.Value?.ToString() ?? string.Empty;
                estado = estadoParam.Value != DBNull.Value ? Convert.ToInt32(estadoParam.Value) : 0;
            }
            catch (SqlException ex)
            {
                salida = $"{ex.Number} - {ex.Message}";
                estado = -1;
            }

            return (accesosXDocenteEE, salida, estado);
        }

        // ++++++++ METODO PARA REGISTRAR INCIDENCIAS DE ASISTENCIAS DE UN FACILITADOR ++++++++++
        //ASINCRONO
        public async Task RegistrarIncidenciasAsistenciaAsync(List<SetIncidenciaAsistencia> incidencias)
        {
            if (incidencias == null || incidencias.Count == 0)
                return;

            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            foreach (var incidencia in incidencias)
            {
                using SqlCommand command = new("SPI_IncidenciasAsistencia_X_Doc", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@IdUsuarioDoc", incidencia.IdUsuarioDoc ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IdCurso", incidencia.IdCurso);
                command.Parameters.AddWithValue(
                    "@Cve_ExperienciaEducativa",
                    string.IsNullOrEmpty(incidencia.Cve_ExperienciaEducativa) ? DBNull.Value : incidencia.Cve_ExperienciaEducativa
                );
                command.Parameters.AddWithValue(
                    "@Cve_ProgramaEducativo",
                    string.IsNullOrEmpty(incidencia.Cve_ProgramaEducativo) ? DBNull.Value : incidencia.Cve_ProgramaEducativo
                );
                command.Parameters.AddWithValue("@FechaEntradaAnterior", incidencia.FechaEntradaAnterior);
                command.Parameters.AddWithValue("@FechaEntrada", incidencia.FechaEntrada ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TiempoMaximoPermitidoHoras", incidencia.TiempoMaximoPermitidoHoras);
                command.Parameters.AddWithValue("@TiempoMaximoPermitidoMin", incidencia.TiempoMaximoPermitidoMin);
                command.Parameters.AddWithValue("@TiempoAusencia", incidencia.TiempoAusencia ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Estatus", incidencia.Estatus);

                SqlParameter salidaParam = new("@Salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                SqlParameter estadoParam = new("@Estado", SqlDbType.Int) { Direction = ParameterDirection.Output };

                command.Parameters.Add(salidaParam);
                command.Parameters.Add(estadoParam);

                try
                {
                    await command.ExecuteNonQueryAsync();

                    string salida = salidaParam.Value?.ToString() ?? string.Empty;
                    int estado = estadoParam.Value != DBNull.Value ? Convert.ToInt32(estadoParam.Value) : 0;

                    // Aquí podrías loggear salida/estado si lo necesitas
                }
                catch (SqlException ex)
                {
                    // Puedes loggear o lanzar excepción si quieres manejar errores por incidencia
                    Console.WriteLine($"Error al registrar incidencia: {ex.Number} - {ex.Message}");
                }
            }

            await connection.CloseAsync();
        }

        /*public async Task<List<SetIncidenciaAsistencia>> DeteccionIncidenciaAsistenciaAsync(string periodoActual)
        {
            List<SetIncidenciaAsistencia> incidenciasDetectadas = new();

            var facilitadores = await FacilitadoresMonitoreadosAsync(periodoActual);

            var diasDescansoUV = await DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual)
            ?? new List<GetCalendarioUV>();

            // Normalizamos los días festivos como DateTime.Date
            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet();


            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;

                var (accesos, salida, estado) = await AccesosEminus_X_docenteEEAsync(idCurso, idUsuarioDoc);

                var accesosOrdenados = accesos
                    .OrderBy(a => DateTime.Parse(a.FechaHoraAcceso))
                    .ToList();

                if (accesosOrdenados.Count == 0)
                    continue;

                var configuraciones = await ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);
                var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == accesosOrdenados[0].cve_Programa);
                if (config == null)
                    continue;

                TimeSpan maxPermitido = new(config.horaMaxAusenciaFacilitador, config.minMaxAusenciaFacilitador, 0);

                for (int i = 0; i < accesosOrdenados.Count - 1; i++)
                {
                    var anterior = DateTime.Parse(accesosOrdenados[i].FechaHoraAcceso);
                    var siguiente = DateTime.Parse(accesosOrdenados[i + 1].FechaHoraAcceso);
                    var diferencia = siguiente - anterior;

                    // Saltar si el día es festivo
                    if (diasFestivos.Contains(anterior.Date))
                        continue;

                    if (diferencia > maxPermitido)
                    {
                        var tiempoExcedido = diferencia - maxPermitido;

                        incidenciasDetectadas.Add(new SetIncidenciaAsistencia
                        {
                            IdUsuarioDoc = idUsuarioDoc,
                            IdCurso = idCurso,
                            Cve_ExperienciaEducativa = seccion,
                            Cve_ProgramaEducativo = accesosOrdenados[0].cve_Programa,
                            FechaEntradaAnterior = anterior,
                            FechaEntrada = siguiente,
                            TiempoMaximoPermitidoHoras = config.horaMaxAusenciaFacilitador,
                            TiempoMaximoPermitidoMin = config.minMaxAusenciaFacilitador,
                            TiempoAusencia = FormatearTiempo(tiempoExcedido),
                            Estatus = 0
                        });
                    }
                }

                // Caso: no ha regresado después del último acceso
                var ultimaEntrada = DateTime.Parse(accesosOrdenados.Last().FechaHoraAcceso);

                // Saltar si el día del último acceso es festivo
                if (diasFestivos.Contains(ultimaEntrada.Date))
                    continue;

                var tiempoDesdeUltimoAcceso = DateTime.Now - ultimaEntrada;

                if (tiempoDesdeUltimoAcceso > maxPermitido)
                {
                    var tiempoExcedido = tiempoDesdeUltimoAcceso - maxPermitido;

                    incidenciasDetectadas.Add(new SetIncidenciaAsistencia
                    {
                        IdUsuarioDoc = idUsuarioDoc,
                        IdCurso = idCurso,
                        Cve_ExperienciaEducativa = seccion,
                        Cve_ProgramaEducativo = accesosOrdenados[0].cve_Programa,
                        FechaEntradaAnterior = ultimaEntrada,
                        FechaEntrada = null,
                        TiempoMaximoPermitidoHoras = config.horaMaxAusenciaFacilitador,
                        TiempoMaximoPermitidoMin = config.minMaxAusenciaFacilitador,
                        TiempoAusencia = null, //FormatearTiempo(tiempoExcedido),
                        Estatus = 1
                    });
                }
            }

            // Guardar todas las incidencias detectadas
            await RegistrarIncidenciasAsistenciaAsync(incidenciasDetectadas);

            return incidenciasDetectadas;
        }
        */

        /*private string FormatearTiempo(TimeSpan ts)
        {
            return $"{ts.Days} días {ts.Hours} hrs {ts.Minutes} min";
        }*/

        #endregion
        
        //*************************************************************************************

        #region Incidencias_de_Actividades_Facilitadores
        // ++++++++++ METODO PARA RECUPERAR TODAS LAS ACTIVIDADES ENTREGADAS POR LOS ESTUDIANTES EN UN CURSO ESPECIFICO. ++++++++++++++++
        //ASINCRONO
        public async Task<List<GetActividadesEntregadas>> ListaActividadesEntregadasXCursoAsync(int idCurso)
        {
            List<GetActividadesEntregadas> ActividadesEntregadas = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_ActividadesEntregadas_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@curso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ActividadesEntregadas.Add(new GetActividadesEntregadas
                    {
                        idActividad = reader["IdActividad"] != DBNull.Value ? Convert.ToInt32(reader["IdActividad"]) : 0,
                        nombreActividad = reader["NombreActividad"]?.ToString(),
                        fechaInicioActividad = reader["FechaInicioActividad"] != DBNull.Value ? Convert.ToDateTime(reader["FechaInicioActividad"]) : DateTime.MinValue,
                        fechaTerminoActividad = reader["FechaTerminoActividad"] != DBNull.Value ? Convert.ToDateTime(reader["FechaTerminoActividad"]) : DateTime.MinValue,
                        porEquipos = reader["PorEquipos"] != DBNull.Value ? Convert.ToInt32(reader["PorEquipos"]) : 0,
                        idActividadEntrega = reader["IdActividadEntrega"] != DBNull.Value ? Convert.ToInt32(reader["IdActividadEntrega"]) : 0,
                        mensajeActEstudiante = reader["texto"]?.ToString(),
                        fechaEntregaActEstudiante = reader["FechaEntregaActEstudiante"] != DBNull.Value ? Convert.ToDateTime(reader["FechaEntregaActEstudiante"]) : DateTime.MinValue,
                        tieneAdjuntos = reader["TieneAdjuntos"] != DBNull.Value ? Convert.ToInt32(reader["TieneAdjuntos"]) : (int?)null,
                        idUsuarioEstudiante = reader["IdUsuarioEstudiante"]?.ToString(),
                        nombreEstudiante = reader["NombreEstudiante"]?.ToString(),
                        idEquipo = reader["IdEquipo"] != DBNull.Value ? Convert.ToInt32(reader["IdEquipo"]) : (int?)null,
                        idCurso = reader["IdCurso"] != DBNull.Value ? Convert.ToInt32(reader["IdCurso"]) : 0,
                        idEstado = reader["IdEstado"] != DBNull.Value ? Convert.ToInt32(reader["IdEstado"]) : 0,
                        visible = reader["Visible"] != DBNull.Value ? Convert.ToInt32(reader["Visible"]) : 0
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener la lista de actividades entregadas.", ex);
            }

            return ActividadesEntregadas;
        }


        // ++++++++++ METODO PARA RECUPERAR TODAS LAS ACTIVIDADES REVISADAS POR UN DOCENTE EN UN CURSO ESPECIFICO. ++++++++++++++++
        //ASINCRONO
        public async Task<List<GetActividadesRevisadas>> ListaActividadesRevisadasXCursoAsync(string idUsuarioDoc, int idCurso)
        {
            List<GetActividadesRevisadas> ActividadesRevisadas = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_ActividadesRevisadas_X_DocenteCurso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));
            command.Parameters.Add(new SqlParameter("@curso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ActividadesRevisadas.Add(new GetActividadesRevisadas
                    {
                        idActividadRevision = int.Parse(reader["IdActividadEntrega"].ToString()),
                        calificacion = reader["Calificacion"] != DBNull.Value
                        ? double.Parse(reader["Calificacion"].ToString())
                        : (double?)null,
                        mensajeActRevision = reader["Texto"].ToString(),
                        fechaRevision = reader["FechaRevision"] != DBNull.Value ? Convert.ToDateTime(reader["FechaRevision"]) : DateTime.MinValue,
                        tieneAdjuntos = reader["TieneAdjuntos"] != DBNull.Value && int.TryParse(reader["TieneAdjuntos"].ToString(), out var tieneAdjuntosVal) ? tieneAdjuntosVal : (int?)null,
                        idActividadEntrega = int.Parse(reader["IdActividadEntrega"].ToString()),
                        idUsuarioDocente = reader["IdUsuario"].ToString(),
                        idUsuarioEstudiante = reader["IdEstudiante"].ToString(),
                        idEquipo = reader["IdEquipo"] != DBNull.Value && int.TryParse(reader["IdEquipo"].ToString(), out var idEquipoVal) ? idEquipoVal : (int?)null,
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        idActividad = int.Parse(reader["IdActividad"].ToString()),
                        visible = int.Parse(reader["Visible"].ToString()),
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
            }

            return ActividadesRevisadas;
        }


        // ********* METODO PARA RECUPERAR TODAS LAS INCIDENCIAS DE ACTIVIDADES DE UN CURSO ESPECIFICIO (PARA DETECCION DE INCIDENCIAS) *********
        //ASINCRONO
        public async Task<List<GetIncidenciaActividad>> ListaIncidenciasActividadesXCursoAsync(int idCurso)
        {
            List<GetIncidenciaActividad> IncidenciasActividades = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_IncidenciasActividades_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    IncidenciasActividades.Add(new GetIncidenciaActividad
                    {
                        idIncidenciaActividad = int.Parse(reader["IdIncidenciaAct"].ToString()),
                        idUsuarioDoc = reader["IdUsuarioDoc"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        Cve_ExperienciaEducativa = reader["Cve_ExperienciaEducativa"].ToString(),
                        Cve_ProgramaEducativo = reader["Cve_ProgramaEducativo"].ToString(),

                        idActividad = int.Parse(reader["IdActividad"].ToString()),
                        nombreActividad = reader["NombreActividad"].ToString(),
                        fechaInicioActividad = reader["FechaInicioActividad"].ToString(),
                        fechaTerminoActividad = reader["FechaTerminoActividad"].ToString(),
                        tipoActividad = reader["TipoActividad"].ToString(),
                        idActividadEntrega = int.Parse(reader["IdActividadEntrega"].ToString()),
                        fechaEntregaActEstudiante = reader["FechaEntregaActEstudiante"].ToString(),
                        matriculaEstudiante = reader["MatriculaEstudiante"].ToString(),
                        nombreEstudiante = reader["NombreEstudiante"].ToString(),

                        idActividadRevision = reader["IdActividadRevision"] == DBNull.Value ? (int?)null : int.Parse(reader["IdActividadRevision"].ToString()),
                        fechaRevisionActDocente = reader["FechaRevisionActDocente"] == DBNull.Value ? null : reader["FechaRevisionActDocente"].ToString(),
                        tiempoRetrasoDocente = reader["TiempoRetrasoDocente"].ToString(),

                        tiempoMaximoPermitidoHoras = int.Parse(reader["TiempoMaximoPermitidoHoras"].ToString()),
                        tiempoMaximoPermitidoMin = int.Parse(reader["TiempoMaximoPermitidoMin"].ToString()),
                        estatus = int.Parse(reader["Estatus"].ToString()),
                        descripcionEstatus = reader["DescripcionEstatus"].ToString()
                    });

                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
            }

            return IncidenciasActividades;
        }


        // ********* METODO PARA REGISTRAR LAS INCIDENCIAS DE ACTIVIDADES - FACILITADORES EN LA BD ************
        //ASINCRONO
        public async Task RegistrarIncidenciasActividadAsync(List<SetIncidenciaActividad> incidencias)
        {
            if (incidencias == null || incidencias.Count == 0)
                return;

            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            foreach (var incidencia in incidencias)
            {
                using SqlCommand command = new("SPIA_IncidenciaActividadDocente", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@IdIncidenciaAct", incidencia.idIncidenciaActividad ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IdUsuarioDoc", incidencia.idUsuarioDoc);
                command.Parameters.AddWithValue("@IdCurso", incidencia.idCurso);
                command.Parameters.AddWithValue(
                    "@Cve_ExperienciaEducativa",
                    string.IsNullOrEmpty(incidencia.Cve_ExperienciaEducativa) ? DBNull.Value : incidencia.Cve_ExperienciaEducativa
                );
                command.Parameters.AddWithValue(
                    "@Cve_ProgramaEducativo",
                    string.IsNullOrEmpty(incidencia.Cve_ProgramaEducativo) ? DBNull.Value : incidencia.Cve_ProgramaEducativo
                );
                command.Parameters.AddWithValue("@IdActividad", incidencia.idActividad);
                command.Parameters.AddWithValue("@NombreActividad", incidencia.nombreActividad);
                command.Parameters.AddWithValue("@FechaInicioActividad", incidencia.fechaInicioActividad);
                command.Parameters.AddWithValue("@FechaTerminoActividad", incidencia.fechaTerminoActividad);
                command.Parameters.AddWithValue("@TipoActividad", incidencia.tipoActividad);
                command.Parameters.AddWithValue("@IdActividadEntrega", incidencia.idActividadEntrega);
                command.Parameters.AddWithValue("@FechaEntregaActEstudiante", incidencia.fechaEntregaActEstudiante);
                command.Parameters.AddWithValue("@MatriculaEstudiante", incidencia.matriculaEstudiante);
                command.Parameters.AddWithValue("@NombreEstudiante", incidencia.nombreEstudiante);
                command.Parameters.AddWithValue("@IdActividadRevision", incidencia.idActividadRevision ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaRevisionActDocente", incidencia.fechaRevisionActDocente ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TiempoRetrasoDocente", incidencia.tiempoRetrasoDocente);

                command.Parameters.AddWithValue("@TiempoMaximoPermitidoHoras", incidencia.tiempoMaximoPermitidoHoras);
                command.Parameters.AddWithValue("@TiempoMaximoPermitidoMin", incidencia.tiempoMaximoPermitidoMin);
                command.Parameters.AddWithValue("@Estatus", incidencia.estatus);
                command.Parameters.AddWithValue("@DescripcionEstatus", incidencia.descripcionEstatus);

                SqlParameter salidaParam = new("@Salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                SqlParameter estadoParam = new("@EstatusHTTP", SqlDbType.Int) { Direction = ParameterDirection.Output };

                command.Parameters.Add(salidaParam);
                command.Parameters.Add(estadoParam);

                try
                {
                    await command.ExecuteNonQueryAsync();

                    string salida = salidaParam.Value?.ToString() ?? string.Empty;
                    int estado = estadoParam.Value != DBNull.Value ? Convert.ToInt32(estadoParam.Value) : 0;

                    // Aquí podrías loggear salida/estado si lo necesitas
                }
                catch (SqlException ex)
                {
                    // Puedes loggear o lanzar excepción si quieres manejar errores por incidencia
                    Console.WriteLine($"Error al registrar incidencia: {ex.Number} - {ex.Message}");
                }
            }

            await connection.CloseAsync();
        }
        #endregion

        //*************************************************************************************

        #region Incidencias_de_Foros_Facilitadores

        // ++++++++++ METODO PARA RECUPERAR TODOS LOS COMENTARIOS DE LOS ESTUDIANTES DE LOS DIFERENTES FOROS DE UN CURSO ESPECIFICO. +++++++++++
        //ASINCRONO
        public async Task<List<GetComentariosForosXCurso>> ListaComentariosForosXCursoAsync(int idCurso)
        {
            List<GetComentariosForosXCurso> ComentariosForos = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_ComentariosForos_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@curso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ComentariosForos.Add(new GetComentariosForosXCurso
                    {
                        idForo = int.Parse(reader["IdForo"].ToString()),
                        nombreForo = reader["NombreForo"].ToString(),
                        fechaInicioForo = reader["FechaInicioForo"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaInicioForo"]),
                        fechaTerminoForo = reader["FechaTerminoForo"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaTerminoForo"]),
                        idComentarioForo = int.Parse(reader["IdComentarioForo"].ToString()),
                        fechaComentarioForoEstudiante = reader["FechaComentarioForoEstudiante"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaComentarioForoEstudiante"]),
                        matriculaEstudiante = reader["MatriculaEstudiante"].ToString(),
                        nombreEstudiante = reader["NombreEstudiante"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString())
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener la lista de actividades entregadas.", ex);
            }

            return ComentariosForos;
        }

        // ++++++++++ METODO PARA RECUPERAR TODOS ESOS COMENTARIOS DE LOS ESTUDIANTES YA LEIDOS POR EL DOCENTE EN UN CURSO ESPECIFICO +++++++++++++++
        //ASINCRONO
        public async Task<List<GetComentariosForosLeidosXDocente>> ListaComentariosForosLeidosXDocenteAsync(string idUsuarioDoc, int idCurso)
        {
            List<GetComentariosForosLeidosXDocente> ComentariosForosLeidos = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_ComentariosForosLeidos_X_DocenteCurso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@IdUsuarioDoc", idUsuarioDoc));
            command.Parameters.Add(new SqlParameter("@curso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ComentariosForosLeidos.Add(new GetComentariosForosLeidosXDocente
                    {
                        idUsuarioComentarioForoLeido = int.Parse(reader["IdUsuarioComentarioForoLeido"].ToString()),
                        idUsuarioDoc = reader["IdUsuario"].ToString(),
                        idComentarioForo = int.Parse(reader["IdComentarioForo"].ToString()),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        fechaComentarioLeido = reader["FechaLeido"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["FechaLeido"]),

                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
            }

            return ComentariosForosLeidos;
        }

        // ********* METODO PARA RECUPERAR TODAS LAS INCIDENCIAS DE ACTIVIDADES DE UN CURSO ESPECIFICIO *********
        //ASINCRONO
        public async Task<List<GetIncidenciaForo>> ListaIncidenciasForosXCursoAsync(int idCurso)
        {
            List<GetIncidenciaForo> IncidenciasForos = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_IncidenciasForos_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    IncidenciasForos.Add(new GetIncidenciaForo
                    {
                        idIncidenciaForo = int.Parse(reader["IdIncidenciaForo"].ToString()),
                        idUsuarioDoc = reader["IdUsuarioDoc"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        Cve_ExperienciaEducativa = reader["Cve_ExperienciaEducativa"].ToString(),
                        Cve_ProgramaEducativo = reader["Cve_ProgramaEducativo"].ToString(),

                        idForo = int.Parse(reader["IdForo"].ToString()),
                        nombreForo = reader["NombreForo"].ToString(),
                        fechaInicioForo = reader["FechaInicioForo"].ToString(),
                        fechaTerminoForo = reader["FechaTerminoForo"].ToString(),
                        idComentarioForo = int.Parse(reader["IdComentarioForo"].ToString()),
                        fechaComentarioForoEstudiante = reader["FechaComentarioForoEstudiante"].ToString(),
                        matriculaEstudiante = reader["MatriculaEstudiante"].ToString(),
                        nombreEstudiante = reader["NombreEstudiante"].ToString(),

                        idUsuarioComentarioForoLeido = reader["IdUsuarioComentarioForoLeido"] == DBNull.Value ? (int?)null : int.Parse(reader["IdUsuarioComentarioForoLeido"].ToString()),
                        fechaComentarioLeido = reader["FechaComentarioLeido"] == DBNull.Value ? null : reader["FechaComentarioLeido"].ToString(),
                        tiempoRetrasoDocente = reader["TiempoRetrasoDocente"].ToString(),

                        tiempoMaximoPermitidoHoras = int.Parse(reader["TiempoMaximoPermitidoHoras"].ToString()),
                        tiempoMaximoPermitidoMin = int.Parse(reader["TiempoMaximoPermitidoMin"].ToString()),
                        estatus = int.Parse(reader["Estatus"].ToString()),
                        descripcionEstatus = reader["DescripcionEstatus"].ToString()
                    });

                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
            }

            return IncidenciasForos;
        }

        // ********* METODO PARA REGISTRAR LAS INCIDENCIAS DE FOROS - FACILITADORES EN LA BD ************
        //ASINCRONO
        public async Task RegistrarIncidenciasForosAsync(List<SetIncidenciaForo> incidencias)
        {
            if (incidencias == null || incidencias.Count == 0)
                return;

            using SqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            foreach (var incidencia in incidencias)
            {
                using SqlCommand command = new("SPIA_IncidenciaForoDocente", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@IdIncidenciaForo", incidencia.idIncidenciaForo ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@IdUsuarioDoc", incidencia.idUsuarioDoc);
                command.Parameters.AddWithValue("@IdCurso", incidencia.idCurso);
                command.Parameters.AddWithValue(
                    "@Cve_ExperienciaEducativa",
                    string.IsNullOrEmpty(incidencia.Cve_ExperienciaEducativa) ? DBNull.Value : incidencia.Cve_ExperienciaEducativa
                );
                command.Parameters.AddWithValue(
                    "@Cve_ProgramaEducativo",
                    string.IsNullOrEmpty(incidencia.Cve_ProgramaEducativo) ? DBNull.Value : incidencia.Cve_ProgramaEducativo
                );

                command.Parameters.AddWithValue("@IdForo", incidencia.idForo);
                command.Parameters.AddWithValue("@NombreForo", incidencia.nombreForo);
                command.Parameters.AddWithValue("@FechaInicioForo", incidencia.fechaInicioForo);
                command.Parameters.AddWithValue("@FechaTerminoForo", incidencia.fechaTerminoForo);

                command.Parameters.AddWithValue("@IdComentarioForo", incidencia.idComentarioForo);
                command.Parameters.AddWithValue("@FechaComentarioForoEstudiante", incidencia.fechaComentarioForoEstudiante);
                command.Parameters.AddWithValue("@MatriculaEstudiante", incidencia.matriculaEstudiante);
                command.Parameters.AddWithValue("@NombreEstudiante", incidencia.nombreEstudiante);
                command.Parameters.AddWithValue("@IdUsuarioComentarioForoLeido", incidencia.idUsuarioComentarioForoLeido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FechaComentarioLeido", incidencia.fechaComentarioLeido ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TiempoRetrasoDocente", incidencia.tiempoRetrasoDocente);

                command.Parameters.AddWithValue("@TiempoMaximoPermitidoHoras", incidencia.tiempoMaximoPermitidoHoras);
                command.Parameters.AddWithValue("@TiempoMaximoPermitidoMin", incidencia.tiempoMaximoPermitidoMin);
                command.Parameters.AddWithValue("@Estatus", incidencia.estatus);
                command.Parameters.AddWithValue("@DescripcionEstatus", incidencia.descripcionEstatus);

                SqlParameter salidaParam = new("@Salida", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };
                SqlParameter estadoParam = new("@EstatusHTTP", SqlDbType.Int) { Direction = ParameterDirection.Output };

                command.Parameters.Add(salidaParam);
                command.Parameters.Add(estadoParam);

                try
                {
                    await command.ExecuteNonQueryAsync();

                    string salida = salidaParam.Value?.ToString() ?? string.Empty;
                    int estado = estadoParam.Value != DBNull.Value ? Convert.ToInt32(estadoParam.Value) : 0;

                    // Aquí podrías loggear salida/estado si lo necesitas
                }
                catch (SqlException ex)
                {
                    // Puedes loggear o lanzar excepción si quieres manejar errores por incidencia
                    Console.WriteLine($"Error al registrar incidencia: {ex.Number} - {ex.Message}");
                }
            }

            await connection.CloseAsync();
        }


        /*public async Task<List<SetIncidenciaForo>> DeteccionIncidenciasForosAsync(string periodoActual)
        {
            // Lista para almacenar todas las incidencias detectadas
            List<SetIncidenciaForo> incidenciasForos = new();

            // Obtener los facilitadores monitoreados en el periodo actual
            var facilitadores = await FacilitadoresMonitoreadosAsync(periodoActual);

            // Obtener los días festivos del calendario institucional para el periodo
            var diasDescansoUV = await DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual) ?? new();

            // Convertir días festivos a un HashSet para búsqueda eficiente
            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet();

            // Obtener las configuraciones de tiempo de revisión por programa educativo
            var configuraciones = await ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);

            // Iterar por cada facilitador (docente)
            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;
                string cvePrograma = facilitador.idProgramaEducativo;

            // Obtener configuración para el programa educativo del docente
            var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == cvePrograma);
                if (config == null)
                    continue;

                // Calcular tiempo máximo de revisión permitido en minutos
                int minutosMaximosRevision = (config.horaMaxForoFacilitador * 60) + config.minMaxForosFacilitador;

                // Obtener los comentarios realizados por los estudiantes en los foros del curso
                var comentariosForos = await ListaComentariosForosXCursoAsync(idCurso);

                // Obtener los comentarios que ya fueron leídos por el docente
                var comentariosLeidos = await ListaComentariosForosLeidosXDocenteAsync(idUsuarioDoc, idCurso);

                // Obtener incidencias ya registradas para evitar duplicados o actualizarlas si es necesario
                var incidenciasExistentes = await ListaIncidenciasForosXCursoAsync(idCurso);

                // Evaluar cada comentario de los estudiantes
                foreach (var comentario in comentariosForos)
                {
                    // Validar que la fecha del comentario sea válida
                    if (!comentario.fechaComentarioForoEstudiante.HasValue)
                        continue;

                    DateTime fechaComentario = comentario.fechaComentarioForoEstudiante.Value;

                    // Ajustar la fecha de inicio de conteo según día hábil
                    //Esta parte aplica:
                    //Si un comentario es enviado un sábado a las 18:00, se empieza a contar el lunes hábil (o martes si el lunes es festivo).
                    //Si la fecha del comentario entra en dia habil, se manda igual la fecha para el conteo.
                    DateTime fechaInicioConteo = diasFestivos.Contains(fechaComentario.Date)
                        || fechaComentario.DayOfWeek == DayOfWeek.Saturday
                        || fechaComentario.DayOfWeek == DayOfWeek.Sunday
                            ? ObtenerSiguienteDiaHabil(fechaComentario, diasFestivos)
                            : fechaComentario;


                    // Buscar si ya hay una incidencia registrada para este comentario
                    var incidenciaExistente = incidenciasExistentes
                        .FirstOrDefault(i => i.idComentarioForo == comentario.idComentarioForo);

                    // Buscar si el docente ya leyó este comentario
                    var comentarioLeido = comentariosLeidos
                        .FirstOrDefault(l => l.idComentarioForo == comentario.idComentarioForo);

                    bool fueLeido = comentarioLeido != null && comentarioLeido.fechaComentarioLeido.HasValue;

                    // Si ya existe una incidencia con estatus 1 (leído fuera de tiempo), no se vuelve a procesar
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 1)
                        continue;

                    // Si existe una incidencia de tipo "no leído" y ahora fue leído, se verifica si fue dentro del tiempo
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 2 && fueLeido)
                    {
                        DateTime fechaLeido = comentarioLeido.fechaComentarioLeido.Value;
                        double minutos = (fechaLeido - fechaInicioConteo).TotalMinutes;

                        // Si fue leído fuera del tiempo permitido, se actualiza la incidencia a estatus 1
                        if (minutos > minutosMaximosRevision)
                        {
                            var retraso = TimeSpan.FromMinutes(minutos - minutosMaximosRevision);

                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idIncidenciaForo = incidenciaExistente.idIncidenciaForo, // Se actualizará
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = comentarioLeido.idUsuarioComentarioForoLeido,
                                fechaComentarioLeido = fechaLeido,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 1,
                                descripcionEstatus = "Comentario leído fuera de tiempo."
                            });
                        }

                        continue;
                    }

                    // Si el comentario no ha sido leído y ya excedió el tiempo permitido, registrar incidencia (estatus 2 - Comentario no leído por el docente.)
                    if (!fueLeido)
                    {
                        TimeSpan retraso = DateTime.Now - fechaInicioConteo;
                        double minutos = retraso.TotalMinutes;

                        if (minutos > minutosMaximosRevision)
                        {
                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idIncidenciaForo = incidenciaExistente?.idIncidenciaForo,
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = null,
                                fechaComentarioLeido = null,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 2,
                                descripcionEstatus = "Comentario no leído por el docente."
                            });
                        }

                        continue;
                    }

                    // Si el comentario fue leído fuera de tiempo, registrar incidencia (estatus 1 - Comentario leido fuera de tiempo)
                    if (fueLeido)
                    {
                        DateTime fechaLeido = comentarioLeido.fechaComentarioLeido.Value;
                        double minutos = (fechaLeido - fechaInicioConteo).TotalMinutes;

                        if (minutos > minutosMaximosRevision)
                        {
                            var retraso = TimeSpan.FromMinutes(minutos - minutosMaximosRevision);

                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = comentarioLeido.idUsuarioComentarioForoLeido,
                                fechaComentarioLeido = fechaLeido,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 1,
                                descripcionEstatus = "Comentario leído fuera de tiempo."
                            });
                        }
                    }
                }
            }

            // Registrar todas las incidencias detectadas en base de datos
            await RegistrarIncidenciasForosAsync(incidenciasForos);

            // Retornar la lista de incidencias generadas
            return incidenciasForos;
        }
        */

        /*public DateTime ObtenerSiguienteDiaHabil(DateTime fecha, HashSet<DateTime> diasFestivos)
        {
            DateTime siguiente = fecha.Date;

            do
            {
                siguiente = siguiente.AddDays(1);
            }
            while (diasFestivos.Contains(siguiente));

            return siguiente;
        }*/


        /*private string FormatearTiempo(TimeSpan ts)
        {
            return $"{ts.Days} días {ts.Hours} hrs {ts.Minutes} min";
        }*/

        #endregion


        //************************************************** METODOS QUE SE USAN MAS DE UNA VEZ EN DIFERENTES ************************************
        //Metodo para consumir un SP en base de datos que recupera los facilitadores monitoreados
        // Recupera solo los facilitadores que se estan monitoreando actualmente, activos y periodo actual.
        //ASINCRONO
        public async Task<List<GetFacilitadoresMonitoreadosActivo>> FacilitadoresMonitoreadosAsync(string periodoActual)
        {
            List<GetFacilitadoresMonitoreadosActivo> facilitadores = new List<GetFacilitadoresMonitoreadosActivo>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_FacilitadoresActivos", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@periodoActual", periodoActual));

                try
                {
                    await connection.OpenAsync(); // Abre la conexión de manera asincrónica

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string idDocente = reader["fk_UsuarioDoc"]?.ToString();
                            int curso = int.Parse(reader["fk_IdCurso"].ToString());
                            string periodo = reader["Periodo"]?.ToString();
                            string nrc = reader["Seccion"] != DBNull.Value ? reader["Seccion"].ToString() : null;
                            string programaEducativo = reader["IdPrograma"] != DBNull.Value ? reader["IdPrograma"].ToString(): null;

                            if (!string.IsNullOrEmpty(idDocente))
                            {
                                facilitadores.Add(new GetFacilitadoresMonitoreadosActivo
                                {
                                    idUsuarioDoc = idDocente,
                                    idCurso = curso,
                                    periodo = periodo,
                                    seccion = nrc,
                                    idProgramaEducativo = programaEducativo
                                });
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al obtener la lista de facilitadores monitoreados.", ex);
                }
            }

            return facilitadores;
        }

        // ++++++++++ METODO PARA RECUPERAR TODAS LAS CONFIGURACIONES DE LOS PROGRAMAS EDUCATIVOS X PERIODO ++++++++++++++++
        //ASINCRONO
        public async Task<List<GetConfiguracionPE>> ListaConfiguracionesPE_X_PeriodoAsync(string periodoActual)
        {
            List<GetConfiguracionPE> configuraciones = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_ConfiguracionesPE_X_Periodo", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@periodoActual", periodoActual));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    configuraciones.Add(new GetConfiguracionPE
                    {
                        IdPEConf = int.Parse(reader["idPEConf"].ToString()),
                        Cve_ProgramaEducativo = reader["fkpk_ProgramaEducativo"].ToString(),
                        Cve_Dependencia = reader["fkpk_Dependencia"].ToString(),
                        IdTipoCalendario = int.Parse(reader["fk_idTipoCal"].ToString()),
                        Cve_Periodo = reader["Periodo"].ToString(),
                        horaMaxAusenciaFacilitador = int.Parse(reader["HoraMaxAusenciaDoc"].ToString()),
                        minMaxAusenciaFacilitador = int.Parse(reader["MinMaxAusenciaDoc"].ToString()),
                        horaMaxRevisionAct_Fac = int.Parse(reader["HoraMaxRevisionAct"].ToString()),
                        minMaxRevisionAct_Fac = int.Parse(reader["MinMaxRevisionAct"].ToString()),
                        horaMaxForoFacilitador = int.Parse(reader["HoraMaxForos"].ToString()),
                        minMaxForosFacilitador = int.Parse(reader["MinMaxForos"].ToString()),
                        horaMaxAusenciaEstudiante = int.Parse(reader["HoraMaxAusenciaEst"].ToString()),
                        minMaxAusenciaEstudiante = int.Parse(reader["MinMaxAusenciaEst"].ToString()),
                        ActividadesSinEntregarEst = int.Parse(reader["ActividadesSinEnt"].ToString()),
                        ExamenesReprobadorEst = int.Parse(reader["ExamenesRep"].ToString()),
                        ForosSinPartiEst = int.Parse(reader["ForosSinPart"].ToString()),
                        UltimaActualizacion = reader["ultimaAct"].ToString()
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener el catálogo de programas educativos.", ex);
            }

            return configuraciones;
        }


        //************************************************** LISTA DE INCIDENCIAS CON PAGINACION ************************************

        public async Task<Paginacion<GetIncidenciaActividad>> ListaIncidenciasActividadesXCursoAsyncPaginacion(
        int idCurso,
        int pageNumber,
        int pageSize,
        string? busquedaGeneral)
        {
            var result = new Paginacion<GetIncidenciaActividad>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<GetIncidenciaActividad> incidencias = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_IncidenciasActividades_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    incidencias.Add(new GetIncidenciaActividad
                    {
                        idIncidenciaActividad = int.Parse(reader["IdIncidenciaAct"].ToString()),
                        idUsuarioDoc = reader["IdUsuarioDoc"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        Cve_ExperienciaEducativa = reader["Cve_ExperienciaEducativa"].ToString(),
                        Cve_ProgramaEducativo = reader["Cve_ProgramaEducativo"].ToString(),

                        idActividad = int.Parse(reader["IdActividad"].ToString()),
                        nombreActividad = reader["NombreActividad"].ToString(),
                        fechaInicioActividad = reader["FechaInicioActividad"].ToString(),
                        fechaTerminoActividad = reader["FechaTerminoActividad"].ToString(),
                        tipoActividad = reader["TipoActividad"].ToString(),
                        idActividadEntrega = int.Parse(reader["IdActividadEntrega"].ToString()),
                        fechaEntregaActEstudiante = reader["FechaEntregaActEstudiante"].ToString(),
                        matriculaEstudiante = reader["MatriculaEstudiante"].ToString(),
                        nombreEstudiante = reader["NombreEstudiante"].ToString(),

                        idActividadRevision = reader["IdActividadRevision"] == DBNull.Value ? (int?)null : int.Parse(reader["IdActividadRevision"].ToString()),
                        fechaRevisionActDocente = reader["FechaRevisionActDocente"] == DBNull.Value ? null : reader["FechaRevisionActDocente"].ToString(),
                        tiempoRetrasoDocente = reader["TiempoRetrasoDocente"].ToString(),

                        tiempoMaximoPermitidoHoras = int.Parse(reader["TiempoMaximoPermitidoHoras"].ToString()),
                        tiempoMaximoPermitidoMin = int.Parse(reader["TiempoMaximoPermitidoMin"].ToString()),
                        estatus = int.Parse(reader["Estatus"].ToString()),
                        descripcionEstatus = reader["DescripcionEstatus"].ToString(),
                        estatusRevisionMonitorPE = reader["EstatusRevisionMonitorPE"] != DBNull.Value
                        && Convert.ToBoolean(reader["EstatusRevisionMonitorPE"])

                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener incidencias de actividades.", ex);
            }

            // Filtro por búsqueda general
            var query = incidencias.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerBusqueda = busquedaGeneral.ToLower();
                query = query.Where(c =>
                    (c.nombreActividad ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.nombreEstudiante ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.matriculaEstudiante ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.tipoActividad ?? "").ToLower().Contains(lowerBusqueda));
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

        public async Task<Paginacion<GetIncidenciaForo>> ListaIncidenciasForosXCursoAsyncPaginacion(
        int idCurso,
        int pageNumber,
        int pageSize,
        string? busquedaGeneral)
        {
            var result = new Paginacion<GetIncidenciaForo>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<GetIncidenciaForo> incidenciasForos = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_IncidenciasForos_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    incidenciasForos.Add(new GetIncidenciaForo
                    {
                        idIncidenciaForo = int.Parse(reader["IdIncidenciaForo"].ToString()),
                        idUsuarioDoc = reader["IdUsuarioDoc"].ToString(),
                        idCurso = int.Parse(reader["IdCurso"].ToString()),
                        Cve_ExperienciaEducativa = reader["Cve_ExperienciaEducativa"].ToString(),
                        Cve_ProgramaEducativo = reader["Cve_ProgramaEducativo"].ToString(),

                        idForo = int.Parse(reader["IdForo"].ToString()),
                        nombreForo = reader["NombreForo"].ToString(),
                        fechaInicioForo = reader["FechaInicioForo"].ToString(),
                        fechaTerminoForo = reader["FechaTerminoForo"].ToString(),
                        idComentarioForo = int.Parse(reader["IdComentarioForo"].ToString()),
                        fechaComentarioForoEstudiante = reader["FechaComentarioForoEstudiante"].ToString(),
                        matriculaEstudiante = reader["MatriculaEstudiante"].ToString(),
                        nombreEstudiante = reader["NombreEstudiante"].ToString(),

                        idUsuarioComentarioForoLeido = reader["IdUsuarioComentarioForoLeido"] == DBNull.Value ? (int?)null : int.Parse(reader["IdUsuarioComentarioForoLeido"].ToString()),
                        fechaComentarioLeido = reader["FechaComentarioLeido"] == DBNull.Value ? null : reader["FechaComentarioLeido"].ToString(),
                        tiempoRetrasoDocente = reader["TiempoRetrasoDocente"].ToString(),

                        tiempoMaximoPermitidoHoras = int.Parse(reader["TiempoMaximoPermitidoHoras"].ToString()),
                        tiempoMaximoPermitidoMin = int.Parse(reader["TiempoMaximoPermitidoMin"].ToString()),
                        estatus = int.Parse(reader["Estatus"].ToString()),
                        descripcionEstatus = reader["DescripcionEstatus"].ToString(),
                        estatusRevisionMonitorPE = reader["EstatusRevisionMonitorPE"] != DBNull.Value
                        && Convert.ToBoolean(reader["EstatusRevisionMonitorPE"])
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener incidencias de foros.", ex);
            }

            // Filtro global
            var query = incidenciasForos.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerBusqueda = busquedaGeneral.ToLower();
                query = query.Where(c =>
                    (c.nombreForo ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.nombreEstudiante ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.Cve_ExperienciaEducativa ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.Cve_ProgramaEducativo ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.matriculaEstudiante ?? "").ToLower().Contains(lowerBusqueda));
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


        public async Task<Paginacion<GetIncidenciaAsistencia>> ListaIncidenciasAsistenciasXCursoAsyncPaginacion(
        int idCurso,
        int pageNumber,
        int pageSize,
        string? busquedaGeneral)
        {
            var result = new Paginacion<GetIncidenciaAsistencia>
            {
                NumeroActualPagina = pageNumber,
                TotalRegistrosXPagina = pageSize
            };

            List<GetIncidenciaAsistencia> incidenciasAsistencias = new();

            using SqlConnection connection = new(_connectionString);
            using SqlCommand command = new("SPS_IncidenciasAsistencia_X_Curso", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

            try
            {
                await connection.OpenAsync();
                using SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    incidenciasAsistencias.Add(new GetIncidenciaAsistencia
                    {
                        IdIncidenciaAsistencia = reader["IdIncidencia"] != DBNull.Value
                        ? Convert.ToInt32(reader["IdIncidencia"])
                        : (int?)null,

                        IdUsuarioDoc = reader["IdUsuarioDoc"]?.ToString(),

                        IdCurso = reader["IdCurso"] != DBNull.Value
                        ? Convert.ToInt32(reader["IdCurso"])
                        : 0,

                        Cve_ExperienciaEducativa = reader["Cve_ExperienciaEducativa"]?.ToString(),
                        Cve_ProgramaEducativo = reader["Cve_ProgramaEducativo"]?.ToString(),

                        FechaEntradaAnterior = reader["FechaEntradaAnterior"] != DBNull.Value
                        ? Convert.ToDateTime(reader["FechaEntradaAnterior"]).ToString() //.ToString("dd/MM/yyyy HH:mm:ss")
                        : string.Empty,

                        FechaEntrada = reader["FechaEntrada"] != DBNull.Value
                        ? Convert.ToDateTime(reader["FechaEntrada"]).ToString() //.ToString("dd/MM/yyyy HH:mm:ss")
                        : null,

                        TiempoMaximoPermitidoHoras = reader["TiempoMaximoPermitidoHoras"] != DBNull.Value
                        ? Convert.ToInt32(reader["TiempoMaximoPermitidoHoras"])
                        : 0,

                        TiempoMaximoPermitidoMin = reader["TiempoMaximoPermitidoMin"] != DBNull.Value
                        ? Convert.ToInt32(reader["TiempoMaximoPermitidoMin"])
                        : 0,

                        TiempoAusencia = reader["TiempoAusencia"] != DBNull.Value
                        ? reader["TiempoAusencia"].ToString()
                        : null,

                        estatusRevisionMonitorPE = reader["EstatusRevisionMonitorPE"] != DBNull.Value
                        && Convert.ToBoolean(reader["EstatusRevisionMonitorPE"])
                    });


                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error al obtener incidencias de foros.", ex);
            }

            // Filtro global
            var query = incidenciasAsistencias.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busquedaGeneral))
            {
                var lowerBusqueda = busquedaGeneral.ToLower();
                query = query.Where(c =>
                    (c.FechaEntrada ?? "").ToLower().Contains(lowerBusqueda) ||
                    (c.FechaEntradaAnterior ?? "").ToLower().Contains(lowerBusqueda));
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


        // Metodo para detectar si hay nuevas incidencias (Actividades, Foros y Asistencia) y si lo hay, actualizar el estatus indicando que hay nuevas incidencias.
        // La idea es ejecutar el metodo unos minutos despues de haber buscado y registrados incidencias, claro si es que hubo.
        public async Task<(string salida, int estado)> DetectarIncidenciasNuevasAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPA_DetectarIncidenciasNuevas", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Parámetros de salida
                SqlParameter salidaParam = new SqlParameter("@Salida", SqlDbType.NVarChar, 300)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(salidaParam);

                SqlParameter estadoParam = new SqlParameter("@EstatusHTTP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(estadoParam);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    string salida = command.Parameters["@Salida"].Value != DBNull.Value
                        ? command.Parameters["@Salida"].Value.ToString()
                        : string.Empty;

                    int estado = command.Parameters["@EstatusHTTP"].Value != DBNull.Value
                        ? Convert.ToInt32(command.Parameters["@EstatusHTTP"].Value)
                        : 0;

                    return (salida, estado);
                }
                catch (SqlException ex)
                {
                    return ($"{ex.Number} - {ex.Message}", -1);
                }
            }
        }

        // Metodo para actualizar el estatus de las incidencias (Actividades, Foros y Asistencia) de pendientes por revisar a revisadas
        // La idea es que este metodo se mande a llamar cuando se mande una notificacion al facilitador de un curso especifico. en ese momento se debe pasar de pendientes
        // por revisar a revisadas, todas las incidencias del curso especifico.
        public async Task<(string salida, int estado)> MarcarIncidenciasRevisadasAsync(int idCurso)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPA_IncidenciasRevisadas_Corte", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Parámetro de entrada
                command.Parameters.Add(new SqlParameter("@idCurso", idCurso));

                // Parámetros de salida
                SqlParameter salidaParam = new SqlParameter("@Salida", SqlDbType.NVarChar, 300)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(salidaParam);

                SqlParameter estadoParam = new SqlParameter("@EstatusHTTP", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                command.Parameters.Add(estadoParam);

                try
                {
                    await connection.OpenAsync();
                    await command.ExecuteNonQueryAsync();

                    string salida = command.Parameters["@Salida"].Value != DBNull.Value
                        ? command.Parameters["@Salida"].Value.ToString()
                        : string.Empty;

                    int estado = command.Parameters["@EstatusHTTP"].Value != DBNull.Value
                        ? Convert.ToInt32(command.Parameters["@EstatusHTTP"].Value)
                        : 0;

                    return (salida, estado);
                }
                catch (SqlException ex)
                {
                    return ($"{ex.Number} - {ex.Message}", -1);
                }
            }
        }


    }
}
