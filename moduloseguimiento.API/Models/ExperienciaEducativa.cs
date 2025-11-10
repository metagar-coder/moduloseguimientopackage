using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace moduloseguimiento.API.Models
{

    //Modelo para obtener la lista de Experiencias educativas X facilitador (Docente).
    public class  GetListaEE_X_facilitador
    {
        public int idUsuarioCurso { get; set; }
        public int idCurso { get; set; }
        public string idUsuario {  get; set; }
        public string nombreCurso { get; set; }
        public string descripcionCurso { get; set; }
        public string cve_Dependencia { get; set; }
        public string cve_ProgramaEducativo { get; set; }
        public string nombreDocente { get; set; }
        public string programaEducativo { get; set; }
        public string dependencia { get; set; }
    }


    //Lista de experiencias educativas monitoreadas X monitorArea especifico.
    //Pantalla principal de lista de experiencias.
    public class GetListaEE_X_CSeguimiento
    {
        public int idCurso { set; get; }
        public string experienciaEducativa { get; set; }
        public string periodo { get; set; }
        public string idUsuarioDocente { get; set; }
        public string nombreDocente {  set; get; }
        public string correoDocente { get; set; }
        public string cve_PE {  get; set; }
        public string programaEducativo { get; set;}
        public string cve_Dependencia { get; set; }
        public string dependencia { get; set; }
        public string idUsuarioMonitorPE { get; set; }
        public string nombreMonitorPE { set; get; }
        public string area {  get; set; }
        public string region { get; set; }
        public int estatusActividades { get; set; }
        public int estatusForos { get; set; }
        public int estatusAsistencias { get; set; }
    }


    // Experiencias Educativas (EE) X Programa Educativo (PE), Periodo y Docente
    public class EEPorPEPeriodoDocente 
    {
        public int id_infoAcademicoPE_EE { set; get; }
        public string cve_EE { get; set; }
        public string ExperienciaEducativa { get; set;}
    }

}
