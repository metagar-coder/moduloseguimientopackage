namespace moduloseguimiento.API.Models
{
    // Modelo para recupera la lista de incidencias de un facilitador, programa educativo, periodo y experiencia educativa especifico.
    public class IncidenciaAcceso
    {
        public string fechaIncidencia {  get; set; }
        public string horaProgramadaEE { get; set; } //Horario de Experiencia Educativa Programada
        public string cve_EEProgramada { get; set; } //Clave de la Experiencia Educativa Programada
        public string experienciaEducativaProgramada { get; set; } 
        public string cve_ProgramaEducativo {  get; set; } //Clave del Programa educativo al que pertenece la EE (Experiencia Educativa) programada.
        public string programaEducativo { get; set; }
        public string cve_EEAccedida { get; set; } //Clave de la Experiencia Educativa que se accedio durante el horario de otra experiencia educativa
        public string experienciaEducativaAccedida { get; set; }
        public string FechaHoraAcceso { get; set; } // Fecha y Hora que se accedio a la experiencia educativa para provocar la incidencia.
    }


    // Modelo para recuperar las incidencias detectadas (Modelo antes del registro de incidencias).
    /*public class GetIncidenciasAccesoDetectadas
    {
        public string fechaIncidencia { get; set;}
        public string IdUsuario { get; set; }
        public string IdCursoAccedido { get; set; }
        public string cve_EEAccedida { get; set; } //Clave de la Experiencia Educativa que se accedio durante el horario de otra experiencia educativa
        public string experienciaEducativaAccedida { get; set; }
        public string FechaHoraAcceso { get; set; } // Fecha y Hora que se accedio a la experiencia educativa para provocar la incidencia.

        public string cve_Periodo {  get; set; }
        public string periodo {  get; set; }
        public int idHorarioEE {  get; set; }
        public string Cve_PE_EEProgramada { get; set; } //Clave del Programa educativo al que pertenece la EE (Experiencia Educativa) programada.
        public string Cve_EEProgramada { get; set; } // NRC. Clave de la Experiencia Educativa Programada
        public string ExperienciaEducativaProgramada { get; set; }
        public string Dia { get; set; }
        public string HorarioConflicto { get; set; } //Horario de la Experiencia Educativa Programada.
    }*/


    //Modelo para registrar las incidencias en la Base de datos
    /*public class SetIncidenciasAccesoDetectadas
    {
        public string fk_Usuario { get; set; }
        public string? cve_ProgramaEducativo { get; set; }
        public string cve_Periodo { get; set; }
        public string cve_EE_Programada { get; set; }
        public int IdHorarioEE { get; set; }
        public string FechaIncidencia { get; set; }
        public string HoraProgramada { get; set; }
        public string cve_EE_Accedida { get; set; }
        public string fechaHoraAcceso { get; set; }
    }*/


    //Modelo para saber el total de incidencias detectadas por facilitador.
    /*public class IncidenciasResumen
    {
        public int TotalIncidencias {  get; set; }
        public Dictionary<string, int> IncidenciasPorUsuario { get; set; } = new();
    }*/



    // ************************************************************************************************************

    // Modelo para saber si hay Incidencias de acceso de un facilitador especifico y de que fecha.
    public class validacionIncidenciasAcceso
    {
        public int ExisteRegistro { get; set; }
        public DateTime? UltimaFechaIncidencia { get; set; }
    }

    public class idCursos
    {
        public int IdCurso { get; set; }
        public string cve_curso { get; set; }
    }

    public class AccesosCursoEspecifico
    {
        public int IdCurso { get; set; }
        public string IdUsuario { get; set; }
        public DateTime FechaHora { get; set; }
    }



    public class IncidenciasResumen
    {
        public int TotalIncidencias { get; set; }
        public List<ResumenIncidenciaFacilitador> Detalles { get; set; } = new();
        public List<SetIncidenciasAccesoDetectadas> IncidenciasDetectadas { get; set; } = new();
    }

    public class ResumenIncidenciaFacilitador
    {
        public string IdUsuarioDoc { get; set; }
        public int TotalIncidencias { get; set; }
    }

    public class SetIncidenciasAccesoDetectadas
    {
        public string fk_Usuario { get; set; }
        public string? cve_ProgramaEducativo { get; set; }
        public string cve_Periodo { get; set; }
        public string cve_EE_Programada { get; set; }
        public int IdHorarioEE { get; set; }
        public string FechaIncidencia { get; set; }
        public string HoraProgramada { get; set; }
        public string cve_EE_Accedida { get; set; }
        public string fechaHoraAcceso { get; set; }
    }


}
