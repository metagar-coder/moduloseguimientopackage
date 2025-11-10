namespace moduloseguimiento.API.Models
{
    public class AccesosEminusXDocenteEE
    {
        public string idUsuarioDoc { get; set; }
        public int idCurso { get; set; }
        public string FechaHoraAcceso { get; set; }
        public string cve_Programa { get; set; }
    }


    public class SetIncidenciaAsistencia
    {
        public string IdUsuarioDoc { get; set; }
        public int IdCurso { get; set; }
        public string? Cve_ExperienciaEducativa { get; set; }
        public string? Cve_ProgramaEducativo { get; set; }
        public DateTime FechaEntradaAnterior { get; set; }
        public DateTime? FechaEntrada { get; set; }
        public int TiempoMaximoPermitidoHoras { get; set; }
        public int TiempoMaximoPermitidoMin { get; set; }
        public string TiempoAusencia { get; set; }
        public int Estatus { get; set; }
    }

    public class GetIncidenciaAsistencia
    {
        public int? IdIncidenciaAsistencia { get; set; }
        public string IdUsuarioDoc { get; set; }
        public int IdCurso { get; set; }
        public string Cve_ExperienciaEducativa { get; set; }
        public string Cve_ProgramaEducativo { get; set; }
        public string FechaEntradaAnterior { get; set; }
        public string? FechaEntrada { get; set; }
        public int TiempoMaximoPermitidoHoras { get; set; }
        public int TiempoMaximoPermitidoMin { get; set; }
        public string? TiempoAusencia { get; set; }
        public bool estatusRevisionMonitorPE { get; set; }
    }

}
