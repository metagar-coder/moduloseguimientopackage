namespace moduloseguimiento.API.Models
{
    public class Estudiante
    {
        public string idUsuarioEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
    }

    public class GetListaEstudiantes
    {
        public string experienciaEducativa { get; set; }
        public string idUsuarioDocente { get; set; }
        public string nombreDocente { get; set; }
        public List<Estudiante> estudiantes { get; set; }
    }

    public class GetDetallesEstudiante
    {
        public int idCurso { get; set; }
        public string idUsuario {  get; set; }
        public string experienciaEducativa { get; set; }
        public string nombreDocente { get;set; }
        public string nombreEstudiante { get; set; }
        public int totalActividades { get; set; }
        public int totalEntregadasATiempo {  get; set; }
        public int totalEntregadasConProrroga { get; set; }
        public int totalPorEntregar {  get; set; }
        public int totalNoEntregadas {  get; set; }
        public int totalParticipacionesForo { get; set; }
        public int totalExamenes { get; set; }
        public int examenesNoPresentados { get; set; }
        public int examenesReprobados { get; set; }

    }
}
