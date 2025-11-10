using Microsoft.VisualBasic;

namespace moduloseguimiento.API.Models
{

    // Horario de clase X Experiencia educativas (Es para la pantalla de carga academica)
    public class GetHorarioEE
    {
        public int id_InfoAcademicoPE_EE { get; set; }
        public string idUsuarioDoc { get; set; }
        public string cve_Periodo { get; set; }
        public string cve_programaEducativa { get; set; }
        public string cve_ExperienciaEducativa { get; set; }
        public string lunes { get; set; }
        public string martes { get; set; }
        public string miercoles { get; set; }
        public string jueves { get; set; }
        public string viernes { get; set; }
        public string sabado { get; set; }
        public string domingo { get; set; }
        public string totalHorasImparte { get; set; }

    }


    // Modelo para recuperar datos de los horarios de los diferentes experiencia educativas de un periodo especifico.
    // Esta recuperacion de horarios es para el analisis y deteccion de incidencias en experiencias educativas.
    public class GetHorariosXPeriodo
    {
        public int id_infoAcademicoPE_EE { get; set; }
        public string idUsuarioDoc { get; set; }
        public string cve_Periodo { get; set; }
        public string periodo { get; set; }
        public string cve_DependenciaEE { get; set; }
        public string cve_PE {  get; set; }
        public string idPEDependencia {  get; set; }
        public string cve_EE { get; set; } // Clave de la experiencia educativa (NRC)
        public string ExperienciaEducativa { get; set; }
        public string lunes { get; set; }
        public string martes { get; set; }
        public string miercoles { get; set; }
        public string jueves { get; set; }
        public string viernes { get; set; }
        public string sabado { get; set; }
        public string domingo { get; set; }
        public string horasTotalesImparte { get; set; }
    }


    //Modelo para el envio de horario de oracle a la base de datos SQL Server.
    public class SetHorarioEE
    {
        public string? pk_Usuario { get; set; }
        public string? NPER {  get; set; }
        public string? APAT { get; set; }
        public string? AMAT { get; set; }
        public string? NOMB { get; set; }
        public int? CVE_REGION_DOC {  get; set; }
        public string? CVE_DEP_ADSCRIPCION_DOC { get; set; }
        public string? CORREOUV { get; set; }
        public string? CORREOALTERNO {  get; set; }

        // Información de la EE(Experiencia Educativa)
        public string? CVE_PERIODO {  get; set; }
        public string? DESC_PERIODO { get; set;}
        public DateTime? FECHA_INICIO { get; set; }
        public DateTime? FECHA_TERMINO { get; set; }
        public int? CVE_REGION_EE {  get; set; }
        public string? CVE_DEP_EE {  get; set; }
        public string? CVE_PROGRAMA_EDUCATIVO {  get; set; }
        public int? fk_IDPEDEPENDENCIA { get; set; }
        public int? CVE_AREA_ACADEMICA {  get; set; }
        public string? MODALIDAD {  get; set; }
        public string? CVE_EE {  get; set; }
        public string? DESC_EE {  get; set; }
        public string? CVE_NIVEL {  get; set; }
        public string? DESC_NIVEL { get; set; }

        // Horarios
        public string? LUNES {  get; set; }
        public string? MARTES {  get; set; }
        public string? MIERCOLES {  get; set; }
        public string? JUEVES {  get; set; }
        public string? VIERNES {  get; set; }
        public string? SABADO {  get; set; }
        public string? DOMINGO {  get; set; }

        public int? HORAS_TOTALES_IMPARTE {  get; set; }

        public int? NMOT {  get; set; }

        public string? DMOT { get; set; }
    
    }

}
