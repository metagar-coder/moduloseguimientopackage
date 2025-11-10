namespace moduloseguimiento.API.Models
{
    public class SetIncidenciaForo
    {
        public int? idIncidenciaForo { get; set; }
        public string idUsuarioDoc { get; set; }
        public int idCurso { get; set; }
        public string Cve_ExperienciaEducativa { get; set; }
        public string Cve_ProgramaEducativo { get; set; }


        public int idForo { get; set; }
        public string nombreForo { get; set; }
        public DateTime? fechaInicioForo { get; set; }
        public DateTime? fechaTerminoForo { get; set; }

        public int idComentarioForo { get; set; }
        public DateTime? fechaComentarioForoEstudiante { get; set; }
        public string matriculaEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int? idUsuarioComentarioForoLeido { get; set; }
        public DateTime? fechaComentarioLeido { get; set; }
        public string tiempoRetrasoDocente { get; set; }

        public int tiempoMaximoPermitidoHoras { get; set; }
        public int tiempoMaximoPermitidoMin { get; set; }
        public int estatus { get; set; }
        public string descripcionEstatus { get; set; }
    }


    public class GetIncidenciaForo
    {
        public int? idIncidenciaForo { get; set; }
        public string idUsuarioDoc { get; set; }
        public int idCurso { get; set; }
        public string? Cve_ExperienciaEducativa { get; set; }
        public string? Cve_ProgramaEducativo { get; set; }


        public int idForo { get; set; }
        public string nombreForo { get; set; }
        public string fechaInicioForo { get; set; }
        public string fechaTerminoForo { get; set; }
        public int idComentarioForo { get; set; }
        public string fechaComentarioForoEstudiante { get; set; }
        public string matriculaEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int? idUsuarioComentarioForoLeido { get; set; }
        public string? fechaComentarioLeido { get; set; }
        public string tiempoRetrasoDocente { get; set; }

        public int tiempoMaximoPermitidoHoras { get; set; }
        public int tiempoMaximoPermitidoMin { get; set; }
        public int estatus { get; set; }
        public string descripcionEstatus { get; set; }
        public bool estatusRevisionMonitorPE { get; set; }
    }

}
