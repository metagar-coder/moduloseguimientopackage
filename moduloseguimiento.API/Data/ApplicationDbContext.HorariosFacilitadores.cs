using Microsoft.Data.SqlClient;
using moduloseguimiento.API.Models;
using System.Data;

namespace moduloseguimiento.API.Data
{
    public partial class ApplicationDbContext
    {

        // Este metodo es para registrar los horarios de los diferentes facilitadores que se vayan registrando para monitorear.
        public async Task<int> RegistroHorarioFacilitador(HorarioEE_Oracle source)
        {
            var (inicio, termino) = ObtenerFechaInicio_Termino(source.PERIODO);

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SPIA_InfoAcademicoPE_EE", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Parámetros de entrada
                command.Parameters.AddWithValue("@Id_InfoAcademicoPE_EE", DBNull.Value); // Insertar
                command.Parameters.AddWithValue("@pk_Usuario", source.USUARIODOC ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NPER", source.NOPER_OVRR.ToString() ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@APAT", source.APPATERNO_TIT ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@AMAT", source.APMATERNO_TIT ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@NOMB", source.NOM_TIT ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fk_CVE_REGION_DOC", (object)source.CVE_REGION_DOC ?? DBNull.Value);
                command.Parameters.AddWithValue("@fk_CVE_DEP_ADSCRIPCION_DOC", source.CVE_ORGANIZACION ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CORREOUV", source.CORREOUV ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CORREOALTERNO", "");

                command.Parameters.AddWithValue("@CVE_PERIODO", source.PERIODO ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DESC_PERIODO", source.DESC_PERI ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@FECHA_INICIO", (object)inicio ?? DBNull.Value);
                command.Parameters.AddWithValue("@FECHA_TERMINO", (object)termino ?? DBNull.Value);
                command.Parameters.AddWithValue("@CVE_REGION_EE", (object)source.CVE_REGION_DOC ?? DBNull.Value);
                command.Parameters.AddWithValue("@CVE_DEP_EE", source.CVE_ORGANIZACION ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CVE_PROGRAMA_EDUCATIVO", source.PROGRAMA ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@fk_IDPEDEPENDENCIA", (source.IDPEDEPENDENCIA == 0 ? (object)DBNull.Value : source.IDPEDEPENDENCIA));
                command.Parameters.AddWithValue("@CVE_AREA_ACADEMICA", int.TryParse(source.COD_AREA, out int valor) ? valor : (object)DBNull.Value);
                command.Parameters.AddWithValue("@MODALIDAD", source.MODALIDAD ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CVE_EE", source.NRC ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DESC_EE", source.EXP_EDUCATIVA ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@CVE_NIVEL", source.NIVEL ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DESC_NIVEL", source.DESC_NIVEL ?? (object)DBNull.Value);

                command.Parameters.AddWithValue("@LUNES", FormatearHorario(source.LU_INI, source.LU_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MARTES", FormatearHorario(source.MA_INI, source.MA_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@MIERCOLES", FormatearHorario(source.MI_INI, source.MI_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@JUEVES", FormatearHorario(source.JU_INI, source.JU_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@VIERNES", FormatearHorario(source.VI_INI, source.VI_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SABADO", FormatearHorario(source.SA_INI, source.SA_FIN) ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@DOMINGO", "");

                command.Parameters.AddWithValue("@HORAS_TOTALES_IMPARTE", int.TryParse(source.TOT_HRS_SEM, out var horas) ? horas : (object)DBNull.Value);
                command.Parameters.AddWithValue("@NMOT", 0);
                command.Parameters.AddWithValue("@DMOT", "");

                // Outputs
                var salidaParam = new SqlParameter("@Salida", SqlDbType.NVarChar, 300) { Direction = ParameterDirection.Output };
                var estatusParam = new SqlParameter("@EstatusHTTP", SqlDbType.Int) { Direction = ParameterDirection.Output };
                command.Parameters.Add(salidaParam);
                command.Parameters.Add(estatusParam);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();

                // Retornamos únicamente el mensaje del SP
                //return salidaParam.Value?.ToString();
                return estatusParam.Value != DBNull.Value ? Convert.ToInt32(estatusParam.Value) : -1; //200 o 500
            }
        }

        private string? FormatearHorario(string? inicio, string? fin)
        {
            // Si alguno es null, lo manejamos
            if (string.IsNullOrEmpty(inicio) || string.IsNullOrEmpty(fin))
                return null;

            if (inicio == "0000" && fin == "0000")
                return null;

            string FormatearHora(string hhmm)
            {
                if (string.IsNullOrEmpty(hhmm) || hhmm.Length != 4)
                    return hhmm ?? string.Empty;

                if (!int.TryParse(hhmm.Substring(0, 2), out int h)) return hhmm;
                if (!int.TryParse(hhmm.Substring(2, 2), out int m)) return hhmm;

                // Redondear minutos a la hora más cercana si quieres simplificar
                if (m >= 30) h += 1;

                return $"{h}:{(m >= 30 ? "00" : m.ToString("D2"))}";
            }

            return $"{FormatearHora(inicio)}-{FormatearHora(fin)}";
        }


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
                fechaInicio = new DateTime(anio, 8, 1);
                fechaTermino = new DateTime(anio + 1, 1, 31);
            }
            else
            {
                throw new ArgumentException("El periodo debe terminar en 51 o 01");
            }

            return (fechaInicio, fechaTermino);
        }


        // Metodo para traer el identificador de la relacion de una dependencia con un programa educativo
        public int ObtenerIdPEDependencia(int idDependencia, string cveProgramaEducativo)
        {
            int idPEDependencia = 0;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            using (SqlCommand command = new SqlCommand("SPS_IdPEDependencia", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // Manejo de parámetros con valores por defecto
                command.Parameters.AddWithValue("@idDependencia", idDependencia > 0 ? idDependencia : 0);
                command.Parameters.AddWithValue("@cveProgramaEducativo", string.IsNullOrEmpty(cveProgramaEducativo) ? DBNull.Value : cveProgramaEducativo);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        idPEDependencia = Convert.ToInt32(result);
                }
                catch (SqlException ex)
                {
                    throw new Exception("Error al obtener IdPEDependencia.", ex);
                }
            }

            return idPEDependencia;
        }


    }
}
