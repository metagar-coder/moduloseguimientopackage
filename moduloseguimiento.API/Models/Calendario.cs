namespace moduloseguimiento.API.Models
{

    //Modelo para insertar los dias festivos de un calendario especifico en la base de datos.
    public class CalendarioUV
    {
        public int IdCalendario { get; set; }
        public int TipoCalendario { get; set; }
        public int IdTipoCal_EMSCCalendarios { get; set; }
        public string Calendario { get; set; }
        public string DescripcionCalendario { get; set; }

        public int IdPeriodo { get; set; }
        public string Cve_Periodo { get; set; }
        public int TipoPeriodo { get; set; }
        public string Periodo { get; set; }

        public int? IdFecha { get; set; }
        public string Fecha { get; set; }
        public string DiaSemana { get; set; }
        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string TipoDia { get; set; }
        public string DescripcionTipoDia { get; set; }
    }


    //Modelos para llenar el modelo CalendarioUV
    //modelos intermedios para deserializar
    public class CalendarioResponse
    {
        public List<CalendarioDTO> content { get; set; }
    }

    public class CalendarioDTO
    {
        public int id { get; set; }
        public int type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<PeriodoDTO> periods { get; set; }
    }

    public class PeriodoDTO
    {
        public int id { get; set; }
        public string name { get; set; }
        public string? cvePeriod { get; set; }
        public int type { get; set; }
        public long startDate { get; set; }
        public long endDate { get; set; }
    }


    public class DiaResponse
    {
        public List<DiaDTO> content { get; set; }
    }

    public class DiaDTO
    {
        public int id { get; set; }
        public long date { get; set; }
        public List<DaysTypes> daysTypes { get; set; }
    }

    public class DaysTypes
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    // *****************************************************************************************************

    //Modelo para recuperar los dias del calendarioUV de un periodo especifico.
    public class GetCalendarioUV
    {
        public int IdCalendario { get; set; }
        public int TipoCalendario { get; set; }
        public int IdTipoCal_EMSCCalendarios { get; set; }
        public string Calendario { get; set; }
        public string DescripcionCalendario { get; set; }

        public int IdPeriodo { get; set; }
        public string Cve_Periodo { get; set; }
        public int TipoPeriodo { get; set; }
        public string Periodo { get; set; }

        public int? IdFecha { get; set; }
        public string Fecha { get; set; }
        public string DiaSemana { get; set; }
        public int Dia { get; set; }
        public int Mes { get; set; }
        public int Anio { get; set; }
        public string TipoDia { get; set; }
        public string DescripcionTipoDia { get; set; }
    }

}
