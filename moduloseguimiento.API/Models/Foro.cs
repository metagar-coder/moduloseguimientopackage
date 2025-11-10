namespace moduloseguimiento.API.Models
{
    public class GetComentariosForosXCurso
    {
        public int idForo {  get; set; }
        public string nombreForo { get; set; }
        public DateTime? fechaInicioForo { get; set; }
        public DateTime? fechaTerminoForo { get; set; }
        public int idComentarioForo { get; set; }
        public DateTime? fechaComentarioForoEstudiante { get; set; }
        public string matriculaEstudiante { get; set; }
        public string nombreEstudiante { get; set; }
        public int idCurso { get; set; }
    }


    public class GetComentariosForosLeidosXDocente
    {
        public int idUsuarioComentarioForoLeido { get; set; }
        public string idUsuarioDoc {  get; set; }
        public int idComentarioForo { get; set; }
        public int idCurso { get; set; }
        public DateTime? fechaComentarioLeido { get; set; }
    }

}
