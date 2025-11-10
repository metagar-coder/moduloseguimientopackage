namespace moduloseguimiento.API.Models
{
    public class HorarioEE_Oracle
    {
        public string? PERIODO { get; set; }
        public string DESC_PERI { get; set; }
        public string? COD_AREA { get; set; }
        public string? AREA_ACAD { get; set; }
        public string? PROGRAMA { get; set; }
        public string? DESC_PROGRAMA { get; set; }
        public string? CAMPUS { get; set; }
        public string? DESC_CAMPUS { get; set; }
        public string? NIVEL { get; set; }
        public string? DESC_NIVEL { get; set; }
        public string? CVE_ORGANIZACION { get; set; }
        public string? DESC_CVE_ORG { get; set; }
        public string? CVE_PROGRAMATICA { get; set; }
        public string? DESC_CVE_PROGRAMATICA { get; set; }
        public string? TIT_SSAOVRR { get; set; }
        public string? NOMBRE_TITULAR { get; set; }
        public string? APPATERNO_TIT { get; set; }
        public string? APMATERNO_TIT { get; set; }
        public string? NOM_TIT { get; set; }
        public int? NOPER_OVRR { get; set; }
        public string? DOCENTE_SSASECT { get; set; }
        public string? APPATERNO_DOCS { get; set; }
        public string? APMATERNO_DOCS { get; set; }
        public string? NOM_DOCS { get; set; }
        public string? NOMBRE_DOCENTE { get; set; }
        public int? NOPER_SS { get; set; }
        public string? NRC { get; set; }
        public string? LCRU { get; set; }
        public string? MATERIA { get; set; }
        public string? CURSO { get; set; }
        public string? EXP_EDUCATIVA { get; set; }
        public string? AREA_FORMACION { get; set; }
        public string? LU_INI { get; set; }
        public string LU_FIN { get; set; }
        public string? MA_INI { get; set; }
        public string? MA_FIN { get; set; }
        public string? MI_INI { get; set; }
        public string? MI_FIN { get; set; }
        public string? JU_INI { get; set; }
        public string? JU_FIN { get; set; }
        public string? VI_INI { get; set; }
        public string? VI_FIN { get; set; }
        public string? SA_INI { get; set; }
        public string? SA_FIN { get; set; }
        public string? TOT_HRS_SEM { get; set; }
        public string? COLL_CODE { get; set; }

        // DATOS EXTRA QUE NO VIENE DE LA BASE DE DATOS DE ORACLE, SINO SE LLENAN CON EL SERVICIO SPARH

        public string? USUARIODOC {  get; set; }
        public string? CORREOUV { get; set; }
        public int? CVE_REGION_DOC { get; set; }
        public int? IDPEDEPENDENCIA { get; set; }
        public string? MODALIDAD { get; set; }


    }


    public class ResultadoHorarios
    {
        public List<HorarioEE_Oracle> Horarios { get; set; } = new List<HorarioEE_Oracle>();
        public string Mensaje { get; set; } = string.Empty;
    }

}
