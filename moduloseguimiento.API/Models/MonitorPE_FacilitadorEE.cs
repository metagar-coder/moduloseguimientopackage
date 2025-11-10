using System.Reflection.Metadata;

namespace moduloseguimiento.API.Models
{
    public class MonitorPE_FacilitadorEE
    {
    }

    public class GetMonitorPE_FacilitadorEE //Modelo para recuperar la lista de monitoresPE con sus respectivos facilitadores y Experiencias Educativas a cargo.
    {
        public string usuarioPE { get; set; }
        public int idTipoPerfil { get; set; }
        public string monitorPE { get; set; }
        public string usuarioArea { get; set; }
        public string monitorArea { get; set; }
        public int idPEDependencia { get; set; }
        public string cve_Dependencia { get; set; }
        public string dependencia { get; set; }
        public string cve_PE { get; set; }
        public string programaEducativo { get; set; }
        public string region { get; set; }
        public List<Facilitador_EE> facilitadoresEE { get; set; }

    }

    public class Facilitador_EE
    {
        public string facilitador { get; set; }
        public string experienciaEducativa { get; set; }
    }


    public class GetFacilitadoresEE
    {
        public int idMonitorDoc { get; set; }
        public string nombreDocente { get; set; }
        public string experienciaEducativa { get; set; }
    }


}
