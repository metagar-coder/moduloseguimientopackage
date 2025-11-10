using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services
{
    public class DeteccionIncidenciasAsistenciaService
    {

        private readonly ApplicationDbContext _context;

        public DeteccionIncidenciasAsistenciaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SetIncidenciaAsistencia>> DeteccionIncidenciaAsistenciaAsync(string periodoActual)
        {
            List<SetIncidenciaAsistencia> incidenciasDetectadas = new();

            var facilitadores = await _context.FacilitadoresMonitoreadosAsync(periodoActual);

            var diasDescansoUV = await _context.DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual)
            ?? new List<GetCalendarioUV>();

            // Normalizamos los días festivos como DateTime.Date
            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet();


            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;

                var (accesos, salida, estado) = await _context.AccesosEminus_X_docenteEEAsync(idCurso, idUsuarioDoc);

                var accesosOrdenados = accesos
                    .OrderBy(a => DateTime.Parse(a.FechaHoraAcceso))
                    .ToList();

                if (accesosOrdenados.Count == 0)
                    continue;

                var configuraciones = await _context.ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);
                var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == accesosOrdenados[0].cve_Programa);
                if (config == null)
                    continue;

                TimeSpan maxPermitido = new(config.horaMaxAusenciaFacilitador, config.minMaxAusenciaFacilitador, 0);

                for (int i = 0; i < accesosOrdenados.Count - 1; i++)
                {
                    var anterior = DateTime.Parse(accesosOrdenados[i].FechaHoraAcceso);
                    var siguiente = DateTime.Parse(accesosOrdenados[i + 1].FechaHoraAcceso);
                    var diferencia = siguiente - anterior;

                    // Saltar si el día es festivo
                    if (diasFestivos.Contains(anterior.Date))
                        continue;

                    if (diferencia > maxPermitido)
                    {
                        var tiempoExcedido = diferencia - maxPermitido;

                        incidenciasDetectadas.Add(new SetIncidenciaAsistencia
                        {
                            IdUsuarioDoc = idUsuarioDoc,
                            IdCurso = idCurso,
                            Cve_ExperienciaEducativa = seccion,
                            Cve_ProgramaEducativo = accesosOrdenados[0].cve_Programa,
                            FechaEntradaAnterior = anterior,
                            FechaEntrada = siguiente,
                            TiempoMaximoPermitidoHoras = config.horaMaxAusenciaFacilitador,
                            TiempoMaximoPermitidoMin = config.minMaxAusenciaFacilitador,
                            TiempoAusencia = FormatearTiempo(tiempoExcedido),
                            Estatus = 0
                        });
                    }
                }

                // Caso: no ha regresado después del último acceso
                var ultimaEntrada = DateTime.Parse(accesosOrdenados.Last().FechaHoraAcceso);

                // Saltar si el día del último acceso es festivo
                if (diasFestivos.Contains(ultimaEntrada.Date))
                    continue;

                var tiempoDesdeUltimoAcceso = DateTime.Now - ultimaEntrada;

                if (tiempoDesdeUltimoAcceso > maxPermitido)
                {
                    var tiempoExcedido = tiempoDesdeUltimoAcceso - maxPermitido;

                    incidenciasDetectadas.Add(new SetIncidenciaAsistencia
                    {
                        IdUsuarioDoc = idUsuarioDoc,
                        IdCurso = idCurso,
                        Cve_ExperienciaEducativa = seccion,
                        Cve_ProgramaEducativo = accesosOrdenados[0].cve_Programa,
                        FechaEntradaAnterior = ultimaEntrada,
                        FechaEntrada = null,
                        TiempoMaximoPermitidoHoras = config.horaMaxAusenciaFacilitador,
                        TiempoMaximoPermitidoMin = config.minMaxAusenciaFacilitador,
                        TiempoAusencia = null, //FormatearTiempo(tiempoExcedido),
                        Estatus = 1
                    });
                }
            }

            // Guardar todas las incidencias detectadas
            await _context.RegistrarIncidenciasAsistenciaAsync(incidenciasDetectadas);

            return incidenciasDetectadas;
        }

        private string FormatearTiempo(TimeSpan ts)
        {
            List<string> partes = new();

            if (ts.Days > 0)
                partes.Add($"{ts.Days} {(ts.Days == 1 ? "día" : "días")}");

            if (ts.Hours > 0)
                partes.Add($"{ts.Hours} {(ts.Hours == 1 ? "hr" : "hrs")}");

            if (ts.Minutes > 0)
                partes.Add($"{ts.Minutes} {(ts.Minutes == 1 ? "min" : "mins")}");

            // Si no hay días, horas ni minutos (todo en 0), al menos mostrar "0 min"
            if (partes.Count == 0)
                return "0 min";

            return string.Join(" ", partes);
        }


    }
}
