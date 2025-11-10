using Microsoft.Identity.Client;

namespace moduloseguimiento.API.Models
{

    public class GetActividadesEntregadas
    {

        public int idActividad { get; set; }
        public string nombreActividad { get; set; }
        public DateTime fechaInicioActividad { get; set; }
        public DateTime fechaTerminoActividad { get; set; }
        public int porEquipos { get; set; }
        public int idActividadEntrega { get; set; }
        public string mensajeActEstudiante { get; set; }
        public DateTime fechaEntregaActEstudiante { get; set; }
        public int? tieneAdjuntos { get; set; }
        public string idUsuarioEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int? idEquipo { get; set; }
        public int idCurso { get; set; }
        public int idEstado { get; set; }
        public int visible { get; set; }

    }

    public class GetActividadesRevisadas
    {
        public int idActividadRevision { get; set; }
        public Double? calificacion { get; set; }
        public string mensajeActRevision { get; set; }
        public DateTime fechaRevision { get; set; }
        public int? tieneAdjuntos { get; set; }
        public int idActividadEntrega { get; set; }
        public string idUsuarioDocente { get; set; }
        public string idUsuarioEstudiante { get; set; }
        public int? idEquipo { get; set; }
        public int idCurso { get; set;}
        public int idActividad { get; set;}
        public int visible { get; set;}
    }

}
