using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services.Interfaces
{
    public interface ICalendar
    {
        Task<string> Calendarios();

        Task<string> DiasCalendario(string IdCalendario, string Periodo);

        Task<List<CalendarioUV>> RegistrarDiasDescanso(string IdCalendario, string Periodo, int IdTipoCalendario);

    }
}
