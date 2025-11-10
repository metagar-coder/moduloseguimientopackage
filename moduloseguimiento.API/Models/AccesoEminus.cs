namespace moduloseguimiento.API.Models
{

    public class GetAccesosEminusCursos
    {
        public int idAccesoSistema { get; set; }
        public string idUsuario { get; set; }
        public string idCurso { get; set; }
        public string fechaHoraAcceso { get; set; }
        public string nombreCurso { get; set; }
        public string idDependencia { get; set; }
        public string seccion { get; set; } //Clave del Experiencia educativa o NRC
        public string periodo { get; set; }
        public string idPrograma { get; set; }

    }

    public class BitacoraAccesosEminus4
    {
        public string? idUsuarioDoc {  get; set; }
        public string? fechaIngreso { get; set; }
        public string? fechaSalida { get; set; }
        public string? tiempoPermanencia { get; set; }
    }

}
