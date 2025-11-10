using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations;

namespace moduloseguimiento.API.Models
{
    public class ProgramaEducativo
    {

        public string Cve_PE { get; set; }
        public string programaEducativo { get; set; }

    }

    public class ConfiguracionProgramaEducativo //Datos para registrar una configuracion para un programa educativos
    {
        [Required(ErrorMessage = "El programa educativo es obligatorio.")]
        public string programaEducativo { get; set; }

        public string dependencia { get; set; }

        [Required(ErrorMessage = "El tipo de calendario es obligatorio.")]
        public int idTipoCalendario { get; set; }

        [Required(ErrorMessage = "La hora máxima de ausencia del facilitador es obligatoria.")]
        public int horaMaxAusenciaFacilitador { get; set; }

        [Required(ErrorMessage = "El minuto máximo de ausencia del facilitador es obligatorio.")]
        public int minMaxAusenciaFacilitador { get; set; }

        [Required(ErrorMessage = "La hora máxima de revisión de actividades por el facilitador es obligatoria.")]
        public int horaMaxRevisionAct_Fac { get; set; }

        [Required(ErrorMessage = "El minuto máximo de revisión de actividades por el facilitador es obligatorio.")]
        public int minMaxRevisionAct_Fac { get; set; }

        [Required(ErrorMessage = "La hora máxima de participación en foros por el facilitador es obligatoria.")]
        public int horaMaxForoFacilitador { get; set; }

        [Required(ErrorMessage = "El minuto máximo de participación en foros por el facilitador es obligatorio.")]
        public int minMaxForosFacilitador { get; set; }

        [Required(ErrorMessage = "La hora máxima de ausencia del estudiante es obligatoria.")]
        public int horaMaxAusenciaEstudiante { get; set; }

        [Required(ErrorMessage = "El minuto máximo de ausencia del estudiante es obligatorio.")]
        public int minMaxAusenciaEstudiante { get; set; }

        [Required(ErrorMessage = "El número de actividades sin entregar por el estudiante es obligatorio.")]
        public int ActividadesSinEntregarEst { get; set; }

        [Required(ErrorMessage = "El número de exámenes reprobados por el estudiante es obligatorio.")]
        public int ExamenesReprobadorEst { get; set; }

        [Required(ErrorMessage = "El número de foros sin participación del estudiante es obligatorio.")]
        public int ForosSinPartiEst { get; set; }

        public string Periodo { get; set; }
    }


    public class GetConfiguracionPE
    {
        public int IdPEConf { get; set; }

        public string Cve_ProgramaEducativo { get; set; }

        public string ProgramaEducativo { get; set; }

        public string Cve_Dependencia { get; set; }

        public string Dependencia { get; set; }

        public int IdTipoCalendario { get; set; }

        public string Calendario { get; set; }

        public int horaMaxAusenciaFacilitador { get; set; }

        public int minMaxAusenciaFacilitador { get; set; }

        public int horaMaxRevisionAct_Fac { get; set; }

        public int minMaxRevisionAct_Fac { get; set; }

        public int horaMaxForoFacilitador { get; set; }

        public int minMaxForosFacilitador { get; set; }

        public int horaMaxAusenciaEstudiante { get; set; }

        public int minMaxAusenciaEstudiante { get; set; }

        public int ActividadesSinEntregarEst { get; set; }

        public int ExamenesReprobadorEst { get; set; }

        public int ForosSinPartiEst { get; set; }

        public string UltimaActualizacion { get; set; }

        public string Cve_Periodo { get; set; }

        public string Periodo { get; set; }
    }

    public class GetCatalogoPE_X_CS //Catalogo de programa educativos X Coordinador de Seguimiento
    {
        public string cve_PE { get; set; }
        public string programaEducativo { get; set; }
    }


    public class GetCatalogoPExCSConPaginacion //Catalogo de programa educativos X Coordinador de Seguiumiento (Con paginación)
    {
        public int idRegion { get; set; }
        public string region { get; set; }
        public string cve_Dependencia { get; set; }
        public string dependencia { get; set; }
        public string cve_PE { get; set; }
        public string programaEducativo { get; set; }
    }

}
