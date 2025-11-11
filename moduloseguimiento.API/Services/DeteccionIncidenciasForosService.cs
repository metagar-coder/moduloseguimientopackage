using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services
{
    public class DeteccionIncidenciasForosService
    {

        private readonly ApplicationDbContext _context;

        public DeteccionIncidenciasForosService(ApplicationDbContext context)
        {
            _context = context;
        }

        // +++++++++ METODO PARA ANALIZAR INCIDENCIAS DE FOROS DE FACILITADORES  +++++++
        public async Task<List<SetIncidenciaForo>> DeteccionIncidenciasForosAsync(string periodoActual)
        {
            // Lista para almacenar todas las incidencias detectadas
            List<SetIncidenciaForo> incidenciasForos = new();

            // 1. Obtener facilitadores monitoreados en el periodo actual
            var facilitadores = await _context.FacilitadoresMonitoreadosAsync(periodoActual);

            // 2. Obtener días de descanso (festivos + fines de semana)
            var diasDescansoUV = await _context.DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual) ?? new();
            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet(); // HashSet para búsquedas rápidas

            // 3. Obtener configuraciones de tiempo máximo de revisión por programa educativo
            var configuraciones = await _context.ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);

            // 4. Iterar cada facilitador
            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;
                string cvePrograma = facilitador.idProgramaEducativo;

                // 5. Obtener configuración del programa educativo correspondiente
                var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == cvePrograma);
                if (config == null)
                    continue;

                // Tiempo máximo de revisión permitido en minutos
                int minutosMaximosRevision = (config.horaMaxForoFacilitador * 60) + config.minMaxForosFacilitador;

                // 6. Obtener comentarios de los estudiantes
                var comentariosForos = await _context.ListaComentariosForosXCursoAsync(idCurso);

                // 7. Obtener comentarios que ya fueron leídos por el docente
                var comentariosLeidos = await _context.ListaComentariosForosLeidosXDocenteAsync(idUsuarioDoc, idCurso);

                // 8. Obtener incidencias existentes para evitar duplicados o actualizarlas
                var incidenciasExistentes = await _context.ListaIncidenciasForosXCursoAsync(idCurso);

                // 9. Evaluar cada comentario
                foreach (var comentario in comentariosForos)
                {
                    if (!comentario.fechaComentarioForoEstudiante.HasValue)
                        continue; // ignorar comentarios sin fecha

                    // --- Fecha base para conteo = fecha de término del foro ---
                    DateTime fechaBase = comentario.fechaTerminoForo ?? DateTime.MinValue;

                    // Ajuste si fecha base cae en día festivo
                    DateTime fechaInicioConteo = diasFestivos.Contains(fechaBase.Date)
                        ? ObtenerSiguienteDiaHabil(fechaBase, diasFestivos)
                        : fechaBase;

                    // Fecha límite = fechaInicioConteo + tiempo máximo permitido
                    DateTime fechaLimiteRevision = fechaInicioConteo
                        .AddHours(config.horaMaxForoFacilitador)
                        .AddMinutes(config.minMaxForosFacilitador);

                    // Verificar si ya existe una incidencia registrada para este comentario
                    var incidenciaExistente = incidenciasExistentes
                        .FirstOrDefault(i => i.idComentarioForo == comentario.idComentarioForo);

                    // Verificar si el comentario ya fue leído por el docente
                    var comentarioLeido = comentariosLeidos
                        .FirstOrDefault(l => l.idComentarioForo == comentario.idComentarioForo);

                    bool fueLeido = comentarioLeido != null && comentarioLeido.fechaComentarioLeido.HasValue;

                    // --- Caso 0: Si la incidencia ya tiene estatus 1, no procesar ---
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 1)
                        continue;

                    // --- Caso 1: Existía incidencia "no leído" (estatus 2) y ahora fue leído ---
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 2 && fueLeido)
                    {
                        DateTime fechaLeido = comentarioLeido.fechaComentarioLeido.Value;
                        double minutos = (fechaLeido - fechaInicioConteo).TotalMinutes;

                        if (minutos > minutosMaximosRevision)
                        {
                            var retraso = TimeSpan.FromMinutes(minutos - minutosMaximosRevision);

                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idIncidenciaForo = incidenciaExistente.idIncidenciaForo, // actualización
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = comentarioLeido.idUsuarioComentarioForoLeido,
                                fechaComentarioLeido = fechaLeido,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 1,
                                descripcionEstatus = "Comentario leído fuera de tiempo."
                            });
                        }

                        continue;
                    }

                    // --- Caso 2: Comentario NO leído y tiempo excedido ---
                    if (!fueLeido)
                    {
                        TimeSpan retraso = DateTime.Now - fechaInicioConteo;
                        double minutos = retraso.TotalMinutes;

                        if (minutos > minutosMaximosRevision)
                        {
                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idIncidenciaForo = incidenciaExistente?.idIncidenciaForo,
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = null,
                                fechaComentarioLeido = null,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 2,
                                descripcionEstatus = "Comentario no leído por el docente."
                            });
                        }

                        continue;
                    }

                    // --- Caso 3: Comentario leído fuera de tiempo ---
                    if (fueLeido)
                    {
                        DateTime fechaLeido = comentarioLeido.fechaComentarioLeido.Value;
                        double minutos = (fechaLeido - fechaInicioConteo).TotalMinutes;

                        if (minutos > minutosMaximosRevision)
                        {
                            var retraso = TimeSpan.FromMinutes(minutos - minutosMaximosRevision);

                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = idCurso,
                                Cve_ProgramaEducativo = cvePrograma,
                                Cve_ExperienciaEducativa = seccion,
                                idForo = comentario.idForo,
                                nombreForo = comentario.nombreForo,
                                fechaInicioForo = comentario.fechaInicioForo,
                                fechaTerminoForo = comentario.fechaTerminoForo,
                                idComentarioForo = comentario.idComentarioForo,
                                fechaComentarioForoEstudiante = comentario.fechaComentarioForoEstudiante,
                                matriculaEstudiante = comentario.matriculaEstudiante,
                                nombreEstudiante = comentario.nombreEstudiante,
                                idUsuarioComentarioForoLeido = comentarioLeido.idUsuarioComentarioForoLeido,
                                fechaComentarioLeido = fechaLeido,
                                tiempoRetrasoDocente = FormatearTiempo(retraso),
                                tiempoMaximoPermitidoHoras = config.horaMaxForoFacilitador,
                                tiempoMaximoPermitidoMin = config.minMaxForosFacilitador,
                                estatus = 1,
                                descripcionEstatus = "Comentario leído fuera de tiempo."
                            });
                        }
                    }
                }
            }

            // 10. Registrar todas las incidencias detectadas en la base de datos
            await _context.RegistrarIncidenciasForosAsync(incidenciasForos);

            // 11. Retornar la lista de incidencias
            return incidenciasForos;
        }


        public DateTime ObtenerSiguienteDiaHabil(DateTime fecha, HashSet<DateTime> diasFestivos)
        {
            DateTime siguiente = fecha.Date;

            do
            {
                siguiente = siguiente.AddDays(1);
            }
            while (diasFestivos.Contains(siguiente));

            return siguiente;
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
