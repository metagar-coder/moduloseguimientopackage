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

        /*public async Task<List<SetIncidenciaForo>> DeteccionIncidenciasForosAsync(string periodoActual)
        {
            // Lista para almacenar todas las incidencias detectadas
            List<SetIncidenciaForo> incidenciasForos = new();

            // Obtener los facilitadores monitoreados en el periodo actual
            var facilitadores = await _context.FacilitadoresMonitoreadosAsync(periodoActual);

            // Obtener los días festivos del calendario institucional para el periodo
            var diasDescansoUV = await _context.DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual) ?? new();

            // Convertir días festivos a un HashSet para búsqueda eficiente
            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet();

            // Obtener las configuraciones de tiempo de revisión por programa educativo
            var configuraciones = await _context.ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);

            // Iterar por cada facilitador (docente)
            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;
                string cvePrograma = facilitador.idProgramaEducativo;

                // Obtener configuración para el programa educativo del docente
                var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == cvePrograma);
                if (config == null)
                    continue;

                // Calcular tiempo máximo de revisión permitido en minutos
                int minutosMaximosRevision = (config.horaMaxForoFacilitador * 60) + config.minMaxForosFacilitador;

                // Obtener los comentarios realizados por los estudiantes en los foros del curso
                var comentariosForos = await _context.ListaComentariosForosXCursoAsync(idCurso);

                // Obtener los comentarios que ya fueron leídos por el docente
                var comentariosLeidos = await _context.ListaComentariosForosLeidosXDocenteAsync(idUsuarioDoc, idCurso);

                // Obtener incidencias ya registradas para evitar duplicados o actualizarlas si es necesario
                var incidenciasExistentes = await _context.ListaIncidenciasForosXCursoAsync(idCurso);

                // Evaluar cada comentario de los estudiantes
                foreach (var comentario in comentariosForos)
                {
                    // Validar que la fecha del comentario sea válida
                    if (!comentario.fechaComentarioForoEstudiante.HasValue)
                        continue;

                    DateTime fechaComentario = comentario.fechaComentarioForoEstudiante.Value;

                    // Ajustar la fecha de inicio de conteo según día hábil
                    //Esta parte aplica:
                    //Si un comentario es enviado un sábado a las 18:00, se empieza a contar el lunes hábil (o martes si el lunes es festivo).
                    //Si la fecha del comentario entra en dia habil, se manda igual la fecha para el conteo.
                    DateTime fechaInicioConteo = diasFestivos.Contains(fechaComentario.Date)
                        || fechaComentario.DayOfWeek == DayOfWeek.Saturday
                        || fechaComentario.DayOfWeek == DayOfWeek.Sunday
                            ? ObtenerSiguienteDiaHabil(fechaComentario, diasFestivos)
                            : fechaComentario;


                    // Buscar si ya hay una incidencia registrada para este comentario
                    var incidenciaExistente = incidenciasExistentes
                        .FirstOrDefault(i => i.idComentarioForo == comentario.idComentarioForo);

                    // Buscar si el docente ya leyó este comentario
                    var comentarioLeido = comentariosLeidos
                        .FirstOrDefault(l => l.idComentarioForo == comentario.idComentarioForo);

                    bool fueLeido = comentarioLeido != null && comentarioLeido.fechaComentarioLeido.HasValue;

                    // Si ya existe una incidencia con estatus 1 (leído fuera de tiempo), no se vuelve a procesar
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 1)
                        continue;

                    // Si existe una incidencia de tipo "no leído" y ahora fue leído, se verifica si fue dentro del tiempo
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 2 && fueLeido)
                    {
                        DateTime fechaLeido = comentarioLeido.fechaComentarioLeido.Value;
                        double minutos = (fechaLeido - fechaInicioConteo).TotalMinutes;

                        // Si fue leído fuera del tiempo permitido, se actualiza la incidencia a estatus 1
                        if (minutos > minutosMaximosRevision)
                        {
                            var retraso = TimeSpan.FromMinutes(minutos - minutosMaximosRevision);

                            incidenciasForos.Add(new SetIncidenciaForo
                            {
                                idIncidenciaForo = incidenciaExistente.idIncidenciaForo, // Se actualizará
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

                    // Si el comentario no ha sido leído y ya excedió el tiempo permitido, registrar incidencia (estatus 2 - Comentario no leído por el docente.)
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

                    // Si el comentario fue leído fuera de tiempo, registrar incidencia (estatus 1 - Comentario leido fuera de tiempo)
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

            // Registrar todas las incidencias detectadas en base de datos
            await _context.RegistrarIncidenciasForosAsync(incidenciasForos);

            // Retornar la lista de incidencias generadas
            return incidenciasForos;
        }*/


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
