namespace moduloseguimiento.API.Models
{
    public class GetFacilitadoresXPeriodo
    {
        public string idUsuario { get; set; }
        public string nombreFacilitador { get; set; }
    }

    public class GetFacilitadoresMonitoreadosActivo
    {
        public string idUsuarioDoc {  get; set; }
        public int idCurso { get; set; }
        public string? periodo { get; set; }
        public string? seccion {  get; set; } //La seccion es el NRC de la experiencia educativa.
        public string? idProgramaEducativo { get; set; }
    }

}
