using moduloseguimiento.API.Data;

namespace moduloseguimiento.API.Services
{
    public class IncidenciasSchedulerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _context;

        // Horarios cada 2 horas, desfasados para cada tipo de detección
        /*private readonly string[] horariosEjecucionIAcceso =
        {
            "00:00", "02:00", "04:00", "06:00", "08:00", "10:00", "12:00", "14:00", "16:00", "18:00", "20:00", "22:00"
        };*/

        private readonly TimeSpan[] horariosEjecucionIAcceso =
        {
            TimeSpan.FromHours(0), TimeSpan.FromHours(2), TimeSpan.FromHours(4),
            TimeSpan.FromHours(6), TimeSpan.FromHours(8), TimeSpan.FromHours(10),
            TimeSpan.FromHours(12), TimeSpan.FromHours(14), TimeSpan.FromHours(16),
            TimeSpan.FromHours(18), TimeSpan.FromHours(20), TimeSpan.FromHours(22),
        };

        /*private readonly string[] horariosEjecucionIAsistencia =
        {
            "01:00", "03:00", "05:00", "07:00", "09:00", "11:00", "13:00", "15:00", "17:00", "19:00", "21:00", "23:00"
        };*/

        private readonly TimeSpan[] horariosEjecucionIAsistencia =
        {
            TimeSpan.FromHours(1), TimeSpan.FromHours(3), TimeSpan.FromHours(5),
            TimeSpan.FromHours(7), TimeSpan.FromHours(9), TimeSpan.FromHours(11),
            TimeSpan.FromHours(13), TimeSpan.FromHours(15), TimeSpan.FromHours(17),
            TimeSpan.FromHours(19), TimeSpan.FromHours(21), TimeSpan.FromHours(23),
        };

        /*private readonly string[] horariosEjecucionIActividad =
        {
            "02:30", "04:30", "06:30", "08:30", "10:30", "12:30", "14:30", "16:30", "18:30", "20:30", "22:30", "00:30"
        };*/

        private readonly TimeSpan[] horariosEjecucionIActividad =
        {
            new TimeSpan(2, 30, 0),  new TimeSpan(4, 30, 0),  new TimeSpan(6, 30, 0),
            new TimeSpan(8, 30, 0),  new TimeSpan(10, 30, 0), new TimeSpan(12, 30, 0),
            new TimeSpan(14, 30, 0), new TimeSpan(16, 30, 0), new TimeSpan(18, 30, 0),
            new TimeSpan(20, 30, 0), new TimeSpan(22, 30, 0), new TimeSpan(0, 30, 0),
        };

        /*private readonly string[] horariosEjecucionIForo =
        {
            "03:15", "05:15", "07:15", "09:15", "11:15", "13:15", "15:15", "17:15", "19:15", "21:15", "23:15", "01:15"
        };*/

        private readonly TimeSpan[] horariosEjecucionIForo =
        {
            new TimeSpan(3, 15, 0),  new TimeSpan(5, 15, 0),  new TimeSpan(7, 15, 0),
            new TimeSpan(9, 15, 0),  new TimeSpan(11, 15, 0), new TimeSpan(13, 15, 0),
            new TimeSpan(15, 15, 0), new TimeSpan(17, 15, 0), new TimeSpan(19, 15, 0),
            new TimeSpan(21, 15, 0), new TimeSpan(23, 15, 0), new TimeSpan(1, 15, 0),
        };

        /*private readonly string[] horariosEjecucionDIN =
        {
            "00:05", "00:35", "01:05", "01:35", "02:05", "02:35", "03:05", "03:35",
            "04:05", "04:35", "05:05", "05:35", "06:05", "06:35", "07:05", "07:35",
            "08:05", "08:35", "09:05", "09:35", "10:05", "10:35", "11:05", "11:35",
            "12:05", "12:35", "13:05", "13:35", "14:05", "14:35", "15:05", "15:35",
            "16:05", "16:35", "17:05", "17:35", "18:05", "18:35", "19:05", "19:35",
            "20:05", "20:35", "21:05", "21:35", "22:05", "22:35", "23:05", "23:35"
        };*/ // Horarios cada 30 minutos para detección de incidencias nuevas y cambio de estatus

        private readonly TimeSpan[] horariosEjecucionDIN =
        {
            new TimeSpan(0, 5, 0),  new TimeSpan(0, 35, 0), new TimeSpan(1, 5, 0),  new TimeSpan(1, 35, 0), new TimeSpan(2, 5, 0),  new TimeSpan(2, 35, 0),
            new TimeSpan(3, 5, 0),  new TimeSpan(3, 35, 0), new TimeSpan(4, 5, 0),  new TimeSpan(4, 35, 0), new TimeSpan(5, 5, 0),  new TimeSpan(5, 35, 0),
            new TimeSpan(6, 5, 0),  new TimeSpan(6, 35, 0), new TimeSpan(7, 5, 0),  new TimeSpan(7, 35, 0), new TimeSpan(8, 5, 0),  new TimeSpan(8, 35, 0),
            new TimeSpan(9, 5, 0),  new TimeSpan(9, 35, 0), new TimeSpan(10, 5, 0), new TimeSpan(10, 35, 0), new TimeSpan(11, 5, 0), new TimeSpan(11, 35, 0),
            new TimeSpan(12, 5, 0), new TimeSpan(12, 35, 0), new TimeSpan(13, 5, 0), new TimeSpan(13, 35, 0), new TimeSpan(14, 5, 0), new TimeSpan(14, 35, 0),
            new TimeSpan(15, 5, 0), new TimeSpan(15, 35, 0), new TimeSpan(16, 5, 0), new TimeSpan(16, 35, 0), new TimeSpan(17, 5, 0), new TimeSpan(17, 35, 0),
            new TimeSpan(18, 5, 0), new TimeSpan(18, 35, 0), new TimeSpan(19, 5, 0), new TimeSpan(19, 35, 0), new TimeSpan(20, 5, 0), new TimeSpan(20, 35, 0),
            new TimeSpan(21, 5, 0), new TimeSpan(21, 35, 0), new TimeSpan(22, 5, 0), new TimeSpan(22, 35, 0), new TimeSpan(23, 5, 0), new TimeSpan(23, 35, 0)
        };

        private string ultimaHoraEjecutadaIAcceso = null;
        private string ultimaHoraEjecutadaIAsistencia = null;
        private string ultimaHoraEjecutadaIActividad = null;
        private string ultimaHoraEjecutadaIForo = null;
        private string ultimaHoraEjecutadaDIN = null;

        public IncidenciasSchedulerService(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            _serviceProvider = serviceProvider;
            _context = context;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var horaActual = DateTime.Now.ToString("HH:mm");

                await EjecutarDeteccionIncidenciasAcceso(horaActual);
                await EjecutarDeteccionIncidenciasAsistencia(horaActual);
                await EjecutarDeteccionIncidenciasActividades(horaActual);
                await EjecutarDeteccionIncidenciasForos(horaActual);
                await EjecutarDeteccionIncidenciasNuevasGENERAL(horaActual);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        //Traslape
        private async Task EjecutarDeteccionIncidenciasAcceso(string horaActual)
        {
            if (TimeSpan.TryParse(horaActual, out var horaActualTs))
            {
                if (horariosEjecucionIAcceso.Contains(horaActualTs) && horaActual != ultimaHoraEjecutadaIAcceso)
                {
                    ultimaHoraEjecutadaIAcceso = horaActual;

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var servicio = scope.ServiceProvider.GetRequiredService<DeteccionIncidenciasAccesoService>();

                        string anioPeriodo = ObtenerAnioPeriodo();

                        var resultado = await servicio.DeteccionIncidenciasAccesoAsync(anioPeriodo);
                        Console.WriteLine($"[✔] Ejecución {horaActual}: {resultado.TotalIncidencias}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[✖] Error ejecutando detección de incidencias: {ex.Message}");
                    }
                }
            }
        }


        private async Task EjecutarDeteccionIncidenciasAsistencia(string horaActual)
        {
            if (TimeSpan.TryParse(horaActual, out var horaActualTs))
            {
                if (horariosEjecucionIAsistencia.Contains(horaActualTs) && horaActual != ultimaHoraEjecutadaIAsistencia)
                {
                    ultimaHoraEjecutadaIAsistencia = horaActual;

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var servicio = scope.ServiceProvider.GetRequiredService<DeteccionIncidenciasAsistenciaService>();

                        string anioPeriodo = ObtenerAnioPeriodo();

                        await servicio.DeteccionIncidenciaAsistenciaAsync(anioPeriodo);
                        Console.WriteLine($"[✔] Detección de asistencias ejecutada a las {horaActual}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[✖] Error en Asistencia: {ex.Message}");
                    }
                }
            }
        }

        private async Task EjecutarDeteccionIncidenciasActividades(string horaActual)
        {
            if (TimeSpan.TryParse(horaActual, out var horaActualTs))
            {
                if (horariosEjecucionIActividad.Contains(horaActualTs) && horaActual != ultimaHoraEjecutadaIActividad)
                {
                    ultimaHoraEjecutadaIActividad = horaActual;

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var servicio = scope.ServiceProvider.GetRequiredService<DeteccionIncidenciasActividadesService>();

                        string anioPeriodo = ObtenerAnioPeriodo();

                        await servicio.DeteccionIncidenciasActividadesAsync(anioPeriodo);
                        Console.WriteLine($"[✔] Detección de Actividades ejecutada a las {horaActual}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[✖] Error en Actividades: {ex.Message}");
                    }
                }
            }
        }

        private async Task EjecutarDeteccionIncidenciasForos(string horaActual)
        {
            if (TimeSpan.TryParse(horaActual, out var horaActualTs))
            {
                if (horariosEjecucionIForo.Contains(horaActualTs) && horaActual != ultimaHoraEjecutadaIForo)
                {
                    ultimaHoraEjecutadaIForo = horaActual;

                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var servicio = scope.ServiceProvider.GetRequiredService<DeteccionIncidenciasForosService>();

                        string anioPeriodo = ObtenerAnioPeriodo();

                        await servicio.DeteccionIncidenciasForosAsync(anioPeriodo);
                        Console.WriteLine($"[✔] Detección de Foros ejecutada a las {horaActual}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[✖] Error en Foros: {ex.Message}");
                    }
                }
            }
        }

        private async Task EjecutarDeteccionIncidenciasNuevasGENERAL(string horaActual)
        {
            if (TimeSpan.TryParse(horaActual, out var horaActualTs))
            {
                if (horariosEjecucionDIN.Contains(horaActualTs) && horaActual != ultimaHoraEjecutadaDIN)
                {
                    ultimaHoraEjecutadaDIN = horaActual;

                    try
                    {
                        var (salida, estado) = await _context.DetectarIncidenciasNuevasAsync();

                        Console.WriteLine($"[✔] Detección de incidencias ejecutada a las {horaActual} | Estado: {estado} | Mensaje: {salida}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[✖] Error en Detección de Incidencias: {ex.Message}");
                    }
                }
            }
        }


        private string ObtenerAnioPeriodo()
        {
            var fechaActual = DateTime.Now;
            int anio = fechaActual.Year;
            int mes = fechaActual.Month;

            string sufijoPeriodo;

            if (mes >= 8 || mes == 1) // Agosto a Enero
            {
                anio++; // Aumentar un año
                sufijoPeriodo = "01";
            }
            else // Febrero a Julio
            {
                sufijoPeriodo = "51";
            }

            return $"{anio}{sufijoPeriodo}";
        }


    }

}
