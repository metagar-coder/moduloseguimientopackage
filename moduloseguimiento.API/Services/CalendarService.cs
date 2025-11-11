using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;
using System.Net.Http;
using System.Text.Json;
using System.Globalization;
using moduloseguimiento.API.Data;

namespace moduloseguimiento.API.Services
{
    public class CalendarService: ICalendar
    {

        private readonly ApplicationDbContext _context;

        public CalendarService(ApplicationDbContext context)
        {
            _context = context;
        }

        //SERVICIOS PARA OBTENER LOS CALENDARIOS DE LA UV.
        // URL base con placeholders para reemplazar
        private const string _calendarios = "WebService";
        private const string _diasCalendario = "WebService";

        public async Task<string> Calendarios()
        {
            try
            {
                // Reemplazar los valores en la ruta
                var url = string.Format(_calendarios);

                var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error al consumir el servicio externo. Código: {response.StatusCode}");
                }

                var contenido = await response.Content.ReadAsStringAsync();
                return contenido;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al contactar con el servidor de los calendarios", ex);
            }
        }

        public async Task<string> DiasCalendario(string IdCalendario, string Periodo)
        {
            try
            {
                // Reemplazar los valores en la ruta
                var url = string.Format(_diasCalendario, IdCalendario, Periodo);

                var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Error al consumir el servicio externo. Código: {response.StatusCode}");
                }

                var contenido = await response.Content.ReadAsStringAsync();
                return contenido;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al contactar con el servidor de los calendarios", ex);
            }
        }

        /*
         + IdCalendario = Es el identificador del calendario que viene en el servicio
         + Periodo = Es el identificador del periodo que viene en relacion con el calendario en el servicio
         + IdTipoCalendario = es el identificador del calendario que viene en el catalogo de la base de datos SQL, en este caso son estos:
            1 - Calendario escolar
            2 - Posgrado
            3 - Acádemico SEA
         */
        public async Task<List<CalendarioUV>> RegistrarDiasDescanso(string IdCalendario, string Periodo, int IdTipoCalendario)
        {
            var resultado = new List<CalendarioUV>();

            // Llama al servicio de calendarios y deserializa
            var calendarioJson = await Calendarios();
            var calendarios = JsonSerializer.Deserialize<CalendarioResponse>(calendarioJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Busca el calendario que coincida con IdCalendario
            var calendario = calendarios?.content?.FirstOrDefault(c => c.id.ToString() == IdCalendario);
            if (calendario == null)
                return resultado;

            // Obtener el periodo directamente del calendario encontrado
            var periodoSeleccionado = calendario.periods?.FirstOrDefault(p => p.id.ToString() == Periodo);
            if (periodoSeleccionado == null)
                return resultado;

            // Obtén días del calendario y periodo específico
            var diasJson = await DiasCalendario(IdCalendario, Periodo);
            var dias = JsonSerializer.Deserialize<DiaResponse>(diasJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dias?.content == null)
                return resultado;

            var culturaEs = new CultureInfo("es-MX");

            // Construye la lista con los días y la info relacionada
            foreach (var dia in dias.content)
            {
                var primerTipoDia = dia.daysTypes?.FirstOrDefault();

                DateTime fecha = DateTimeOffset.FromUnixTimeMilliseconds(dia.date).DateTime;
                string diaSemanaEsp = culturaEs.DateTimeFormat.GetDayName(fecha.DayOfWeek);

                resultado.Add(new CalendarioUV
                {
                    IdCalendario = calendario.id,
                    TipoCalendario = calendario.type,
                    IdTipoCal_EMSCCalendarios = IdTipoCalendario,
                    Calendario = calendario.name,
                    DescripcionCalendario = calendario.description,

                    IdPeriodo = periodoSeleccionado.id,
                    Cve_Periodo = periodoSeleccionado.cvePeriod,
                    TipoPeriodo = periodoSeleccionado.type,
                    Periodo = periodoSeleccionado.name,

                    IdFecha = dia.id,
                    Fecha = fecha.ToString("yyyy-MM-dd").Replace(" ", ""),
                    DiaSemana = diaSemanaEsp,
                    Dia = fecha.Day,
                    Mes = fecha.Month,
                    Anio = fecha.Year,
                    TipoDia = primerTipoDia?.name,
                    DescripcionTipoDia = primerTipoDia?.description
                });
            }


            // 5. Agrega los fines de semana manualmente
            DateTime inicioPeriodo = DateTimeOffset.FromUnixTimeMilliseconds(periodoSeleccionado.startDate).Date;
            DateTime finPeriodo = DateTimeOffset.FromUnixTimeMilliseconds(periodoSeleccionado.endDate).Date;

            //DateTime inicioPeriodo = DateTimeOffset.FromUnixTimeMilliseconds(1612137600000).Date;
            //DateTime finPeriodo = DateTimeOffset.FromUnixTimeMilliseconds(1627689600000).Date;

            for (var fecha = inicioPeriodo; fecha <= finPeriodo; fecha = fecha.AddDays(1))
            {
                if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                {
                    // Evita duplicar si ya fue registrado por el servicio (por ejemplo, feriado en fin de semana)
                    bool yaExiste = resultado.Any(r => r.Fecha == fecha.ToString("yyyy-MM-dd"));

                    if (!yaExiste)
                    {
                        string diaSemanaEsp = culturaEs.DateTimeFormat.GetDayName(fecha.DayOfWeek);

                        resultado.Add(new CalendarioUV
                        {
                            IdCalendario = calendario.id,
                            TipoCalendario = calendario.type,
                            IdTipoCal_EMSCCalendarios = IdTipoCalendario,
                            Calendario = calendario.name,
                            DescripcionCalendario = calendario.description,

                            IdPeriodo = periodoSeleccionado.id,
                            Cve_Periodo = periodoSeleccionado.cvePeriod,
                            TipoPeriodo = periodoSeleccionado.type,
                            Periodo = periodoSeleccionado.name,

                            IdFecha = null,
                            Fecha = fecha.ToString("yyyy-MM-dd"),
                            DiaSemana = diaSemanaEsp,
                            Dia = fecha.Day,
                            Mes = fecha.Month,
                            Anio = fecha.Year,
                            TipoDia = "Weekend",
                            DescripcionTipoDia = "Fin de semana"
                        });
                    }
                }
            }


            _context.RegistrarDiasDescansoUV(resultado, out string salidaRegistro, out int estadoRegistro);

            return resultado;
        }

    }
}
