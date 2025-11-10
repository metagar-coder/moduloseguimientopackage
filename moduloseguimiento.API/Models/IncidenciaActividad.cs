namespace moduloseguimiento.API.Models
{

    public class SetIncidenciaActividad
    {
        public int? idIncidenciaActividad {  get; set; }
        public string idUsuarioDoc { get; set; }
        public int idCurso { get; set; }
        public string Cve_ExperienciaEducativa { get; set; }
        public string Cve_ProgramaEducativo { get; set; }


        public int idActividad { get; set; }
        public string nombreActividad { get; set; }
        public DateTime? fechaInicioActividad { get; set; }
        public DateTime? fechaTerminoActividad { get; set; }
        public string tipoActividad { get; set; }
        public int idActividadEntrega { get; set; }
        public DateTime? fechaEntregaActEstudiante { get; set; }
        public string matriculaEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int? idActividadRevision { get; set; }
        public DateTime? fechaRevisionActDocente { get; set; }
        public string tiempoRetrasoDocente { get; set; }

        public int tiempoMaximoPermitidoHoras { get; set; }
        public int tiempoMaximoPermitidoMin { get; set; }
        public int estatus { get; set; }
        public string descripcionEstatus { get; set; }
    }


    public class GetIncidenciaActividad
    {
        public int? idIncidenciaActividad { get; set; }
        public string idUsuarioDoc { get; set; }
        public int idCurso { get; set; }
        public string? Cve_ExperienciaEducativa { get; set; }
        public string? Cve_ProgramaEducativo { get; set; }


        public int idActividad { get; set; }
        public string nombreActividad { get; set; }
        public string fechaInicioActividad { get; set; }
        public string fechaTerminoActividad { get; set; }
        public string tipoActividad { get; set; }
        public int idActividadEntrega { get; set; }
        public string fechaEntregaActEstudiante { get; set; }
        public string matriculaEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int? idActividadRevision { get; set; }
        public string? fechaRevisionActDocente { get; set; }
        public string tiempoRetrasoDocente { get; set; }

        public int tiempoMaximoPermitidoHoras { get; set; }
        public int tiempoMaximoPermitidoMin { get; set; }
        public int estatus { get; set; }
        public string descripcionEstatus { get; set; }
        public bool estatusRevisionMonitorPE { get; set; }
    }

}
