using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services
{
    public class DeteccionIncidenciasActividadesService
    {

        private readonly ApplicationDbContext _context;

        public DeteccionIncidenciasActividadesService(ApplicationDbContext context)
        {
            _context = context;
        }


        // +++++++++ METODO PARA ANALIZAR INCIDENCIAS DE ACTIVIDADES DE FACILITADORES  +++++++
        public async Task<List<SetIncidenciaActividad>> DeteccionIncidenciasActividadesAsync(string periodoActual)
        {
            // Lista para acumular todas las incidencias detectadas
            List<SetIncidenciaActividad> incidenciasActividades = new();

            // 1. Recuperar los facilitadores monitoreados en este periodo
            var facilitadores = await _context.FacilitadoresMonitoreadosAsync(periodoActual);

            // 2. Recuperar días festivos y descansos del calendario UV
            var diasDescansoUV = await _context.DiasDescansoCalendarioUV_X_PeriodoAsync(periodoActual)
                                 ?? new List<GetCalendarioUV>();

            var diasFestivos = diasDescansoUV
                .Where(d => !string.IsNullOrEmpty(d.Fecha))
                .Select(d => DateTime.Parse(d.Fecha).Date)
                .ToHashSet(); // HashSet para búsquedas rápidas

            // 3. Recorrer cada facilitador
            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;
                int idCurso = facilitador.idCurso;
                string seccion = facilitador.seccion;
                string cvePrograma = facilitador.idProgramaEducativo;

                // 4. Recuperar actividades entregadas y revisadas
                var actividadesEntregadas = await _context.ListaActividadesEntregadasXCursoAsync(idCurso);
                var actividadesRevisadas = await _context.ListaActividadesRevisadasXCursoAsync(idUsuarioDoc, idCurso);

                // 5. Recuperar incidencias ya registradas
                var incidenciasExistentes = await _context.ListaIncidenciasActividadesXCursoAsync(idCurso);

                // 6. Obtener configuración del programa educativo
                var configuraciones = await _context.ListaConfiguracionesPE_X_PeriodoAsync(periodoActual);
                var config = configuraciones.FirstOrDefault(x => x.Cve_ProgramaEducativo == cvePrograma);

                if (config == null)
                    return incidenciasActividades; // Si no hay configuración, no se hace nada

                // Tiempo máximo permitido para revisión en minutos
                int minutosMaximosRevision = (config.horaMaxRevisionAct_Fac * 60) + config.minMaxRevisionAct_Fac;

                // 7. Recorrer cada actividad entregada
                foreach (var actividad in actividadesEntregadas)
                {
                    if (actividad.fechaTerminoActividad == DateTime.MinValue)
                        continue; // Ignorar actividades sin fecha de término

                    // Buscar si hay revisión por el docente
                    var revision = actividadesRevisadas.FirstOrDefault(r =>
                        r.idActividadEntrega == actividad.idActividadEntrega &&
                        r.idUsuarioEstudiante == actividad.idUsuarioEstudiante &&
                        (actividad.porEquipos == 0 || r.idEquipo == actividad.idEquipo)
                    );

                    // Base de cálculo = fecha de término de la actividad
                    DateTime fechaBase = actividad.fechaTerminoActividad;

                    // Ajustar si cae en día festivo o descanso
                    if (diasFestivos.Contains(fechaBase.Date))
                        fechaBase = ObtenerSiguienteDiaHabil(fechaBase, diasFestivos);

                    // Fecha límite de revisión = fecha base + tiempo permitido
                    DateTime fechaLimiteRevision = fechaBase
                        .AddHours(config.horaMaxRevisionAct_Fac)
                        .AddMinutes(config.minMaxRevisionAct_Fac);

                    // Verificar si ya existe incidencia
                    var incidenciaExistente = incidenciasExistentes
                        .FirstOrDefault(i => i.idActividadEntrega == actividad.idActividadEntrega);

                    // 🚨 Si ya tiene estatus = 1, no se procesa más
                    if (incidenciaExistente != null && incidenciaExistente.estatus == 1)
                        continue;

                    // -------- CASO 1: No hay revisión --------
                    if (revision == null || revision.fechaRevision <= DateTime.MinValue)
                    {
                        // Si ya pasó el plazo, registrar incidencia como "no revisada"
                        if (DateTime.Now > fechaLimiteRevision)
                        {
                            incidenciasActividades.Add(new SetIncidenciaActividad
                            {
                                idIncidenciaActividad = incidenciaExistente?.idIncidenciaActividad,
                                idUsuarioDoc = idUsuarioDoc,
                                idCurso = actividad.idCurso,
                                Cve_ExperienciaEducativa = seccion,
                                Cve_ProgramaEducativo = cvePrograma,
                                idActividad = actividad.idActividad,
                                nombreActividad = actividad.nombreActividad,
                                fechaInicioActividad = actividad.fechaInicioActividad,
                                fechaTerminoActividad = actividad.fechaTerminoActividad,
                                tipoActividad = actividad.porEquipos == 0 ? "Individual" : "En equipo",
                                idActividadEntrega = actividad.idActividadEntrega,
                                fechaEntregaActEstudiante = actividad.fechaEntregaActEstudiante,
                                matriculaEstudiante = actividad.idUsuarioEstudiante,
                                nombreEstudiante = actividad.nombreEstudiante,
                                idActividadRevision = null,
                                fechaRevisionActDocente = null,
                                tiempoRetrasoDocente = FormatearTiempo(DateTime.Now - fechaLimiteRevision),
                                tiempoMaximoPermitidoHoras = config.horaMaxRevisionAct_Fac,
                                tiempoMaximoPermitidoMin = config.minMaxRevisionAct_Fac,
                                estatus = 2, // No revisada
                                descripcionEstatus = "Actividad no revisada."
                            });
                        }

                        continue; // Pasar a la siguiente actividad
                    }

                    // -------- CASO 2: Revisión realizada --------
                    if (revision.fechaRevision > fechaLimiteRevision)
                    {
                        // Revisada fuera de plazo -> crear o actualizar incidencia
                        TimeSpan retrasoExcedido = revision.fechaRevision - fechaLimiteRevision;

                        incidenciasActividades.Add(new SetIncidenciaActividad
                        {
                            idIncidenciaActividad = incidenciaExistente?.idIncidenciaActividad,
                            idUsuarioDoc = idUsuarioDoc,
                            idCurso = actividad.idCurso,
                            Cve_ExperienciaEducativa = seccion,
                            Cve_ProgramaEducativo = cvePrograma,
                            idActividad = actividad.idActividad,
                            nombreActividad = actividad.nombreActividad,
                            fechaInicioActividad = actividad.fechaInicioActividad,
                            fechaTerminoActividad = actividad.fechaTerminoActividad,
                            tipoActividad = actividad.porEquipos == 0 ? "Individual" : "En equipo",
                            idActividadEntrega = actividad.idActividadEntrega,
                            fechaEntregaActEstudiante = actividad.fechaEntregaActEstudiante,
                            matriculaEstudiante = actividad.idUsuarioEstudiante,
                            nombreEstudiante = actividad.nombreEstudiante,
                            idActividadRevision = revision.idActividadRevision,
                            fechaRevisionActDocente = revision.fechaRevision,
                            tiempoRetrasoDocente = FormatearTiempo(retrasoExcedido),
                            tiempoMaximoPermitidoHoras = config.horaMaxRevisionAct_Fac,
                            tiempoMaximoPermitidoMin = config.minMaxRevisionAct_Fac,
                            estatus = 1, // Revisada fuera de plazo
                            descripcionEstatus = "Actividad revisada fuera de tiempo."
                        });
                    }

                    // -------- CASO 3: Revisión dentro del plazo --------
                    // No hacer nada, no se registra incidencia
                }
            }

            // 8. Registrar todas las incidencias detectadas
            await _context.RegistrarIncidenciasActividadAsync(incidenciasActividades);

            // 9. Retornar la lista de incidencias
            return incidenciasActividades;
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
