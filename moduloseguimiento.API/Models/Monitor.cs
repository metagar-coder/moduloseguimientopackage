using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace moduloseguimiento.API.Models
{
    public class MonitorPE
    {
        public string usuario { get; set; }

        public string nombre { get; set; }

        public string correoInstitucional { get; set; }

        public string rol { get; set; }

        public int idTipoPerfil { get; set; }

        public string nombreMonitorArea { get; set; }

        public int idPEDependencia { get; set; }

        public string claveProgramaEducativo { get; set; }

        public string programaEducativo { get; set; }

        public int Cve_dependencia { get; set; }

        public string dependencia { get; set; }

        public string region { get; set; }

    }

    public class NewMonitorPE
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres.")]
        public string nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string apellidoPat { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        public string apellidoMat { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "El correo institucional no tiene un formato válido.")]
        public string correoInstitucional { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo alterno no tiene un formato válido.")]
        public string? correoAlterno { get; set; }

        [Phone(ErrorMessage = "El teléfono de contacto no tiene un formato válido.")]
        public string? telContacto { get; set; }


        [Required(ErrorMessage = "El número de personal es obligatorio.")]
        public int numPersonal { get; set; } = 0;

        [Required(ErrorMessage = "La dependencia es obligatoria.")]
        public string dependencia { get; set; } = string.Empty;

        [Required(ErrorMessage = "El programa educativo es obligatorio.")]
        public string programaEducativo { get; set; } = string.Empty;
    }


    public class SendFiltrosListaMonitoresPE
    {
        public int pageNumber { get; set; } = 1;
        public int pageSize { get; set; } = 3;
        public string? busquedaGeneral { get; set; } = string.Empty;
        public string? rol { get; set; } = string.Empty;
        public string? dependencia { get; set; } = string.Empty;
        public string? region { get; set; } = string.Empty;
    }

    public class EliminarMonitorPE
    {
        public string usuario { get; set; }
        public int IdPEDependencia { get; set; }
    }

    public class ActualizarMonitorPE
    {
        public string usuario { get; set; }
        public string Cve_dependencia { get; set; }
        public string programaEducativo { get; set; }
        public int idPEDependencia { get; set; }
    }


    public class NewMonitorPE_EE //Modelo para registrar la relacion de un monitorPE con una Experiencia Educativa.
    {
        [Required(ErrorMessage = "El campo idPEDependencia es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El idPEDependencia debe ser mayor a 0.")]
        public int idPEDependencia { get; set; }

        [Required(ErrorMessage = "El Usuario del monitorPE es obligatoria.")]
        public string usuarioPE { get; set; }

        [Required(ErrorMessage = "El usuario del facilitador EE es obligatoria.")]
        public string usuarioDoc { get; set; }

        [Required(ErrorMessage = "El campo idCurso es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El idCurso debe ser mayor a 0.")]
        public int idCurso { get; set; }
    }


}

