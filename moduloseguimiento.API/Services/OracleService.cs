using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;
using Newtonsoft.Json.Linq;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Threading.Tasks;

namespace moduloseguimiento.API.Services
{
    public class OracleService
    {
        private readonly string _connectionString;
        private readonly IGetSPARHData _getSPARHDATA;
        private readonly ApplicationDbContext _dbContext;
        private readonly SeguridadSIIUService _seguridadService;

        public OracleService(IConfiguration configuration, IGetSPARHData getSPARHData, ApplicationDbContext dbContext, SeguridadSIIUService seguridadService)
        {
            _connectionString = configuration.GetConnectionString("OracleDb");
            _getSPARHDATA = getSPARHData;
            _dbContext = dbContext;
            _seguridadService = seguridadService;
        }


        #region Metodo_Oracle_Produccion
        //Metodo para traer los horarios de un facilitador especifico de Oracle (Datos crudos)
        public async Task<ResultadoHorarios> Horario_Facilitador_Oracle_Produccion(string idUsuarioDoc, Dictionary<string, object>? parametros = null)
        {
            OracleConnection conexion = null;

            var resultados = new ResultadoHorarios();

            try
            {
                conexion = _seguridadService.ConectaSIIU("segSWV_VEMI");

                if (conexion == null || conexion.State != ConnectionState.Open)
                {
                    resultados.Mensaje = "No se pudo conectar a la BD del SIIU o no tiene permisos.";
                    return resultados;
                }

                    // *********************************************************************************************************
                    // Buscando informacion extra del usuario del servicio de SPARH
                    JObject? sparhData = null;
                    int maxRetries = 3;

                    for (int i = 0; i < maxRetries; i++)
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(idUsuarioDoc))
                            {
                                var sparhResponse = await _getSPARHDATA.SPARHData(idUsuarioDoc);
                                sparhData = sparhResponse.Contenido as JObject;

                                if (sparhData != null) break; // salió bien, no hace más reintentos
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Intento {i + 1} fallido: {ex.Message}");
                        }

                        // Espera antes del siguiente intento
                        await Task.Delay(15000); // 15 segundos
                    }

                    // *********************************************************************************************************

                    string anioPeriodo = ObtenerAnioPeriodo();

                    // Cadena de consulta de oracle para traer los horarios de clase del facilitador especifico.
                    string query = @"SELECT A.*, 
                                        B.MODALIDAD
                                FROM SWV_PROG_ACADEMICA A
                                INNER JOIN SWV_PROG_ACADEMICA_RH B
                                    ON B.NRC = A.NRC
                                   AND B.CURSO = A.CURSO
                                   AND B.PERIODO = A.PERIODO
                                   AND B.NOPER_OVRR = A.NOPER_OVRR
                                WHERE A.PERIODO = " + anioPeriodo + @"
                                  AND A.NOPER_OVRR = " + (sparhData?["noper"]?.ToString() ?? "0");

                using var command = new OracleCommand(query, conexion);

                if (parametros != null)
                    {
                        foreach (var param in parametros)
                        {
                            command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                        }
                    }

                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var fila = new HorarioEE_Oracle
                        {
                            PERIODO = GetNullableString(reader, "PERIODO"),
                            DESC_PERI = GetNullableString(reader, "DESC_PERI"),
                            COD_AREA = GetNullableString(reader, "COD_AREA"),
                            AREA_ACAD = GetNullableString(reader, "AREA_ACAD"),
                            PROGRAMA = GetNullableString(reader, "PROGRAMA"),
                            DESC_PROGRAMA = GetNullableString(reader, "DESC_PROGRAMA"),
                            CAMPUS = GetNullableString(reader, "CAMPUS"),
                            DESC_CAMPUS = GetNullableString(reader, "DESC_CAMPUS"),
                            NIVEL = GetNullableString(reader, "NIVEL"),
                            DESC_NIVEL = GetNullableString(reader, "DESC_NIVEL"),
                            CVE_ORGANIZACION = GetNullableString(reader, "CVE_ORGANIZACION"),
                            DESC_CVE_ORG = GetNullableString(reader, "DESC_CVE_ORG"),
                            CVE_PROGRAMATICA = GetNullableString(reader, "CVE_PROGRAMATICA"),
                            DESC_CVE_PROGRAMATICA = GetNullableString(reader, "DESC_CVE_PROGRAMATICA"),
                            TIT_SSAOVRR = GetNullableString(reader, "TIT_SSAOVRR"),
                            NOMBRE_TITULAR = GetNullableString(reader, "NOMBRE_TITULAR"),
                            APPATERNO_TIT = GetNullableString(reader, "APPATERNO_TIT"),
                            APMATERNO_TIT = GetNullableString(reader, "APMATERNO_TIT"),
                            NOM_TIT = GetNullableString(reader, "NOM_TIT"),

                            NOPER_OVRR = reader["NOPER_OVRR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOPER_OVRR"]),

                            DOCENTE_SSASECT = GetNullableString(reader, "DOCENTE_SSASECT"),
                            APPATERNO_DOCS = GetNullableString(reader, "APPATERNO_DOCS"),
                            APMATERNO_DOCS = GetNullableString(reader, "APMATERNO_DOCS"),
                            NOM_DOCS = GetNullableString(reader, "NOM_DOCS"),
                            NOMBRE_DOCENTE = GetNullableString(reader, "NOMBRE_DOCENTE"),

                            NOPER_SS = reader["NOPER_SS"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOPER_SS"]),

                            NRC = GetNullableString(reader, "NRC"),
                            LCRU = GetNullableString(reader, "LCRU"),
                            MATERIA = GetNullableString(reader, "MATERIA"),
                            CURSO = GetNullableString(reader, "CURSO"),
                            EXP_EDUCATIVA = GetNullableString(reader, "EXP_EDUCATIVA"),
                            AREA_FORMACION = GetNullableString(reader, "AREA_FORMACION"),
                            LU_INI = GetNullableString(reader, "LU_INI"),
                            LU_FIN = GetNullableString(reader, "LU_FIN"),
                            MA_INI = GetNullableString(reader, "MA_INI"),
                            MA_FIN = GetNullableString(reader, "MA_FIN"),
                            MI_INI = GetNullableString(reader, "MI_INI"),
                            MI_FIN = GetNullableString(reader, "MI_FIN"),
                            JU_INI = GetNullableString(reader, "JU_INI"),
                            JU_FIN = GetNullableString(reader, "JU_FIN"),
                            VI_INI = GetNullableString(reader, "VI_INI"),
                            VI_FIN = GetNullableString(reader, "VI_FIN"),
                            SA_INI = GetNullableString(reader, "SA_INI"),
                            SA_FIN = GetNullableString(reader, "SA_FIN"),
                            TOT_HRS_SEM = GetNullableString(reader, "TOT_HRS_SEM"),
                            COLL_CODE = GetNullableString(reader, "COLL_CODE"),


                            // DATOS EXTRA QUE NO VIENE DE LA BASE DE DATOS DE ORACLE, SINO SE LLENAN CON EL SERVICIO SPARH
                            USUARIODOC = sparhData?["cvelogin"]?.ToString() ?? "",
                            CORREOUV = sparhData?["correo"]?.ToString() ?? "",
                            CVE_REGION_DOC = int.Parse(sparhData?["nzon"]?.ToString() ?? ""),
                            IDPEDEPENDENCIA = _dbContext.ObtenerIdPEDependencia(
                                int.TryParse(GetNullableString(reader, "CVE_ORGANIZACION"), out int idDependencia) ? idDependencia : 0,
                                GetNullableString(reader, "PROGRAMA")
                            ),
                            MODALIDAD = GetNullableString(reader, "MODALIDAD")

                        };

                        resultados.Horarios.Add(fila);
                    }

                    return resultados;
            }
            catch (Exception ex)
            {
                resultados.Mensaje = $"Error en Horario_Facilitador_Oracle_Produccion: {ex.Message}";
                return resultados;
            }
            finally
            {
                if (conexion != null)
                {
                    _seguridadService.DesconectaSIIU(conexion);
                    Console.WriteLine("🔌 Conexión cerrada.");
                }
            }
        }
        #endregion



        #region Metodos_Oracle_Desarrollo
        
        // Método para probar conexión
        /*public async Task<string> ProbarConexionAsync()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();

                // Consulta de prueba (por ejemplo obtener fecha/hora del servidor Oracle)
                using var command = new OracleCommand("SELECT SYSDATE FROM DUAL", connection);
                var result = await command.ExecuteScalarAsync();

                return $"Conexión exitosa. Fecha/hora Oracle: {result}";
            }
            catch (Exception ex)
            {
                return $"Error de conexión: {ex.Message}";
            }
        }
        */

        //Metodo para traer los horarios de un facilitador especifico de Oracle_Desarrollo (Datos crudos)
        /*public async Task<List<HorarioEE_Oracle>> Horario_Facilitador_Oracle(string idUsuarioDoc, Dictionary<string, object>? parametros = null)
        {

            // *********************************************************************************************************
            // Buscando informacion extra del usuario del servicio de SPARH
            JObject? sparhData = null;
            int maxRetries = 3;

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(idUsuarioDoc))
                    {
                        var sparhResponse = await _getSPARHDATA.SPARHData(idUsuarioDoc);
                        sparhData = sparhResponse.Contenido as JObject;

                        if (sparhData != null) break; // salió bien, no hace más reintentos
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Intento {i + 1} fallido: {ex.Message}");
                }

                // Espera antes del siguiente intento
                await Task.Delay(15000); // 15 segundos
            }

            // *********************************************************************************************************

            string anioPeriodo = ObtenerAnioPeriodo();

            // Cadena de consulta de oracle para traer los horarios de clase del facilitador especifico.
            //string query = @"SELECT * FROM SWV_PROG_ACADEMICA WHERE PERIODO = " + anioPeriodo + " AND NOPER_OVRR=" + sparhData?["noper"]?.ToString() ?? "";
            string query = @"SELECT A.*, 
                                    B.MODALIDAD
                            FROM SWV_PROG_ACADEMICA A
                            INNER JOIN SWV_PROG_ACADEMICA_RH B
                                ON B.NRC = A.NRC
                               AND B.CURSO = A.CURSO
                               AND B.PERIODO = A.PERIODO
                               AND B.NOPER_OVRR = A.NOPER_OVRR
                            WHERE A.PERIODO = " + anioPeriodo + @"
                              AND A.NOPER_OVRR = " + (sparhData?["noper"]?.ToString() ?? "0");

            var resultados = new List<HorarioEE_Oracle>();

            using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();

            using var command = new OracleCommand(query, connection);

            if (parametros != null)
            {
                foreach (var param in parametros)
                {
                    command.Parameters.Add(new OracleParameter(param.Key, param.Value ?? DBNull.Value));
                }
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var fila = new HorarioEE_Oracle
                {
                    PERIODO = GetNullableString(reader, "PERIODO"),
                    DESC_PERI = GetNullableString(reader, "DESC_PERI"),
                    COD_AREA = GetNullableString(reader, "COD_AREA"),
                    AREA_ACAD = GetNullableString(reader, "AREA_ACAD"),
                    PROGRAMA = GetNullableString(reader, "PROGRAMA"),
                    DESC_PROGRAMA = GetNullableString(reader, "DESC_PROGRAMA"),
                    CAMPUS = GetNullableString(reader, "CAMPUS"),
                    DESC_CAMPUS = GetNullableString(reader, "DESC_CAMPUS"),
                    NIVEL = GetNullableString(reader, "NIVEL"),
                    DESC_NIVEL = GetNullableString(reader, "DESC_NIVEL"),
                    CVE_ORGANIZACION = GetNullableString(reader, "CVE_ORGANIZACION"),
                    DESC_CVE_ORG = GetNullableString(reader, "DESC_CVE_ORG"),
                    CVE_PROGRAMATICA = GetNullableString(reader, "CVE_PROGRAMATICA"),
                    DESC_CVE_PROGRAMATICA = GetNullableString(reader, "DESC_CVE_PROGRAMATICA"),
                    TIT_SSAOVRR = GetNullableString(reader, "TIT_SSAOVRR"),
                    NOMBRE_TITULAR = GetNullableString(reader, "NOMBRE_TITULAR"),
                    APPATERNO_TIT = GetNullableString(reader, "APPATERNO_TIT"),
                    APMATERNO_TIT = GetNullableString(reader, "APMATERNO_TIT"),
                    NOM_TIT = GetNullableString(reader, "NOM_TIT"),

                    NOPER_OVRR = reader["NOPER_OVRR"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOPER_OVRR"]),

                    DOCENTE_SSASECT = GetNullableString(reader, "DOCENTE_SSASECT"),
                    APPATERNO_DOCS = GetNullableString(reader, "APPATERNO_DOCS"),
                    APMATERNO_DOCS = GetNullableString(reader, "APMATERNO_DOCS"),
                    NOM_DOCS = GetNullableString(reader, "NOM_DOCS"),
                    NOMBRE_DOCENTE = GetNullableString(reader, "NOMBRE_DOCENTE"),

                    NOPER_SS = reader["NOPER_SS"] == DBNull.Value ? 0 : Convert.ToInt32(reader["NOPER_SS"]),

                    NRC = GetNullableString(reader, "NRC"),
                    LCRU = GetNullableString(reader, "LCRU"),
                    MATERIA = GetNullableString(reader, "MATERIA"),
                    CURSO = GetNullableString(reader, "CURSO"),
                    EXP_EDUCATIVA = GetNullableString(reader, "EXP_EDUCATIVA"),
                    AREA_FORMACION = GetNullableString(reader, "AREA_FORMACION"),
                    LU_INI = GetNullableString(reader, "LU_INI"),
                    LU_FIN = GetNullableString(reader, "LU_FIN"),
                    MA_INI = GetNullableString(reader, "MA_INI"),
                    MA_FIN = GetNullableString(reader, "MA_FIN"),
                    MI_INI = GetNullableString(reader, "MI_INI"),
                    MI_FIN = GetNullableString(reader, "MI_FIN"),
                    JU_INI = GetNullableString(reader, "JU_INI"),
                    JU_FIN = GetNullableString(reader, "JU_FIN"),
                    VI_INI = GetNullableString(reader, "VI_INI"),
                    VI_FIN = GetNullableString(reader, "VI_FIN"),
                    SA_INI = GetNullableString(reader, "SA_INI"),
                    SA_FIN = GetNullableString(reader, "SA_FIN"),
                    TOT_HRS_SEM = GetNullableString(reader, "TOT_HRS_SEM"),
                    COLL_CODE = GetNullableString(reader, "COLL_CODE"),


                    // DATOS EXTRA QUE NO VIENE DE LA BASE DE DATOS DE ORACLE, SINO SE LLENAN CON EL SERVICIO SPARH
                    USUARIODOC = sparhData?["cvelogin"]?.ToString() ?? "",
                    CORREOUV = sparhData?["correo"]?.ToString() ?? "",
                    CVE_REGION_DOC = int.Parse(sparhData?["nzon"]?.ToString() ?? ""),
                    IDPEDEPENDENCIA = _dbContext.ObtenerIdPEDependencia(
                        int.TryParse(GetNullableString(reader, "CVE_ORGANIZACION"), out int idDependencia) ? idDependencia : 0,
                        GetNullableString(reader, "PROGRAMA")
                    ),
                    MODALIDAD = GetNullableString(reader, "MODALIDAD")

                };

                resultados.Add(fila);
            }

            return resultados;
        }
        */
        
        #endregion


        // Metodo para mapear los datos de oracle a un modelo especifico y despues mostrarlos.
        public async Task<SetHorarioEE> HorariosFacilitador(HorarioEE_Oracle source)
        {

            var (inicio, termino) = ObtenerFechaInicio_Termino(source.PERIODO);

            return new SetHorarioEE
            {
                // Usuario/docente
                pk_Usuario = source.USUARIODOC,
                NPER = source.NOPER_OVRR.ToString(),
                APAT = source.APPATERNO_TIT,
                AMAT = source.APMATERNO_TIT,
                NOMB = source.NOM_TIT,
                CVE_REGION_DOC = source.CVE_REGION_DOC,
                CVE_DEP_ADSCRIPCION_DOC = source.CVE_ORGANIZACION,
                CORREOUV = source.CORREOUV,
                CORREOALTERNO = "",

                // Información de la EE
                CVE_PERIODO = source.PERIODO,
                DESC_PERIODO = source.DESC_PERI,
                FECHA_INICIO = inicio,
                FECHA_TERMINO = termino,
                CVE_REGION_EE = source.CVE_REGION_DOC,
                CVE_DEP_EE = source.CVE_ORGANIZACION,
                CVE_PROGRAMA_EDUCATIVO = source.PROGRAMA,
                fk_IDPEDEPENDENCIA = source.IDPEDEPENDENCIA,
                CVE_AREA_ACADEMICA = int.TryParse(source.COD_AREA, out int valor) ? valor : (int?)null,
                MODALIDAD = source.MODALIDAD,
                CVE_EE = source.NRC,
                DESC_EE = source.EXP_EDUCATIVA,
                CVE_NIVEL = source.NIVEL,
                DESC_NIVEL = source.DESC_NIVEL,

                // Horarios (concatenas inicio y fin por día)
                LUNES = FormatearHorario(source.LU_INI, source.LU_FIN),
                MARTES = FormatearHorario(source.MA_INI, source.MA_FIN),
                MIERCOLES = FormatearHorario(source.MI_INI, source.MI_FIN),
                JUEVES = FormatearHorario(source.JU_INI, source.JU_FIN),
                VIERNES = FormatearHorario(source.VI_INI, source.VI_FIN),
                SABADO = FormatearHorario(source.SA_INI, source.SA_FIN),
                DOMINGO = "", // si no hay campo para domingo en Oracle

                HORAS_TOTALES_IMPARTE = int.TryParse(source.TOT_HRS_SEM, out var horas) ? horas : 0,

                NMOT = 0,
                DMOT = ""
            };
        }


        // Metodo para registrar los horarios de un facilitador con la estructura deseada a SQL Server.
        public async Task<int> RegistrarHorariosFacilitadorAsync(string idUsuarioDoc)
        {
            // -------------------------- ORACLE - DESARROLLO -------------------------------------------
            //var horariosOracle = await Horario_Facilitador_Oracle(idUsuarioDoc);


            // -------------------------- ORACLE - PRODUCCION -------------------------------------------
            var horariosOracle = await Horario_Facilitador_Oracle_Produccion(idUsuarioDoc.ToLower());


            if (horariosOracle == null || horariosOracle.Horarios.Count == 0)
                return 4; // No se encontraron horarios para este facilitador ⛔

            bool algunExitoso = false;
            bool algunFallido = false;

            foreach (var horario in horariosOracle.Horarios)
            {
                try
                {
                    int estatus = await _dbContext.RegistroHorarioFacilitador(horario);

                    if (estatus == 200 || estatus == 204)
                        algunExitoso = true; // Registro exitoso de este horario
                    else
                        algunFallido = true; // Fallo al registrar este horario
                }
                catch
                {
                    algunFallido = true; // Error al registrar este horario
                }
            }

            if (algunExitoso && !algunFallido)
                return 1; // Todos los horarios se registraron correctamente ✅
            if (algunExitoso && algunFallido)
                return 2; // Registro parcial: algunos horarios no se pudieron registrar ⚠️
            if (!algunExitoso && algunFallido)
                return 3; // No se pudieron registrar los horarios ❌

            return 3; // Caso por defecto: fallo ❌
        }


        // Metodo para registrar o actualizar los horarios de los diferentes facilitadores registrados a monitorear del periodo actual
        public async Task<int> ActualizarHorariosFacilitadoresAsync(string monitorArea)
        {
            string salida;
            int estado;
            var periodoActual = ObtenerAnioPeriodo();

            // Recuperar facilitadores a partir del monitor y periodo actual
            var facilitadores = _dbContext.FacilitadoresXPeriodo(
                monitorArea,
                periodoActual,
                out salida,
                out estado);

            if (facilitadores == null || facilitadores.Count == 0)
                return 0; // No hay facilitadores a procesar ⛔

            int todosExitosos = 0;
            int parciales = 0;
            int todosFallidos = 0;

            // Procesar cada facilitador
            foreach (var facilitador in facilitadores)
            {
                // Aquí usamos el servicio Oracle para registrar sus horarios
                int resultado = await RegistrarHorariosFacilitadorAsync(facilitador.idUsuario.ToLower());

                switch (resultado)
                {
                    case 1: // Todos los horarios se registraron correctamente
                        todosExitosos++;
                        break;
                    case 2: // Algunos horarios se registraron, otros fallaron
                        parciales++;
                        break;
                    case 3: // Todos fallaron
                    case 4: // No se encontraron horarios
                        todosFallidos++;
                        break;
                }
            }

            // Puedes devolver un código según el balance
            if (todosExitosos > 0 && parciales == 0 && todosFallidos == 0)
                return 1; // ✅ Todos correctos
            if (todosExitosos > 0 && (parciales > 0 || todosFallidos > 0))
                return 2; // ⚠️ Algunos bien, otros mal
            if (todosExitosos == 0 && (parciales > 0 || todosFallidos > 0))
                return 3; // ❌ Ninguno correcto
            return 0; // No procesados
        }


        #region Metodos_Extra
        // Metodo para convertir este horario  Inicio= 0900 -> 9:00, Fin= 1059 -> 11:00 
        private string? FormatearHorario(string? inicio, string? fin)
        {
            if (string.IsNullOrEmpty(inicio) || string.IsNullOrEmpty(fin))
                return null;

            if (inicio == "0000" && fin == "0000")
                return null;

            string FormatearHora(string? hhmm)
            {
                if (string.IsNullOrEmpty(hhmm) || hhmm.Length != 4)
                    return hhmm ?? "";

                if (!int.TryParse(hhmm.Substring(0, 2), out int h))
                    h = 0;
                if (!int.TryParse(hhmm.Substring(2, 2), out int m))
                    m = 0;

                // Redondear minutos a la hora más cercana
                if (m >= 30) h += 1;

                return $"{h}:{(m >= 30 ? "00" : m.ToString("D2"))}";
            }

            return $"{FormatearHora(inicio)}-{FormatearHora(fin)}";
        }


        // Metodo para obtener la fecha de inicio y termino de un periodo especifico.
        private (DateTime FechaInicio, DateTime FechaTermino) ObtenerFechaInicio_Termino(string periodo)
        {
            // Validamos que el periodo no sea nulo ni vacío
            if (string.IsNullOrWhiteSpace(periodo) || periodo.Length < 6)
                throw new ArgumentException("El periodo debe tener al menos 6 caracteres en formato YYYYxx");

            // Tomamos el año y sufijo (últimos 2 caracteres)
            int anio = int.Parse(periodo.Substring(0, 4));
            string sufijo = periodo.Substring(periodo.Length - 2);

            DateTime fechaInicio;
            DateTime fechaTermino;

            if (sufijo == "51")
            {
                // Febrero a Julio del mismo año
                fechaInicio = new DateTime(anio, 2, 1);
                fechaTermino = new DateTime(anio, 7, 31);
            }
            else if (sufijo == "01")
            {
                // Agosto (año actual) a Enero (año siguiente)
                fechaInicio = new DateTime(anio -1, 8, 1);
                fechaTermino = new DateTime(anio, 1, 31);
            }
            else
            {
                throw new ArgumentException("El periodo debe terminar en 51 o 01");
            }

            return (fechaInicio, fechaTermino);
        }

        // Metodo para convertir datos que vengan asi "" a null.
        private string? GetNullableString(IDataReader reader, string columnName)
        {
            var value = reader[columnName];
            if (value == DBNull.Value) return null;

            var str = value.ToString()?.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }


        // Metodo para obtener el periodo actual
        private string ObtenerAnioPeriodo()
        {
            var fechaActual = DateTime.Now;
            int anio = fechaActual.Year;
            int mes = fechaActual.Month;

            string sufijoPeriodo;

            if (mes >= 8 || mes == 1) // Agosto a Enero
            {
                anio++; // Aumentar un año
                sufijoPeriodo = "01";
            }
            else // Febrero a Julio
            {
                sufijoPeriodo = "51";
            }

            return $"{anio}{sufijoPeriodo}";
        }
        #endregion

    }
}
