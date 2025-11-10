using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using System.Globalization;
using System.Text.Json;

namespace moduloseguimiento.API.Services
{
    public class DeteccionIncidenciasAccesoService
    {

        private readonly ApplicationDbContext _context;

        public DeteccionIncidenciasAccesoService(ApplicationDbContext context)
        {
            _context = context;
        }

        //Servicio para detenccion de Incidencias de Acceso de los facilitadores monitoreados (Del periodo Actual)
        //ASINCRONO
        public async Task<IncidenciasResumen> DeteccionIncidenciasAccesoAsync(string anioPeriodo)
        {
            // Se crea un objeto resumen que almacenará los resultados generales:
            // - Detalles: incidencias por facilitador.
            // - IncidenciasDetectadas: lista completa de incidencias encontradas.
            var resumen = new IncidenciasResumen
            {
                Detalles = new List<ResumenIncidenciaFacilitador>(),
                IncidenciasDetectadas = new List<SetIncidenciasAccesoDetectadas>()
            };

            // 1️ Obtener la lista de facilitadores monitoreados para el periodo especificado.
            var facilitadores = await _context.FacilitadoresMonitoreadosAsync(anioPeriodo);

            // 2️ Iterar sobre cada facilitador para analizar sus accesos e incidencias.
            foreach (var facilitador in facilitadores)
            {
                string idUsuarioDoc = facilitador.idUsuarioDoc;  // ID del docente/facilitador
                int contadorIncidenciasFacilitador = 0;          // Contador local por facilitador

                // 3️ Verificar si el facilitador tiene incidencias previas registradas.
                var validacion = await _context.ValidarIncidenciasAccesoAsync(anioPeriodo, idUsuarioDoc);
                DateTime? ultimaFechaIncidencia = validacion?.UltimaFechaIncidencia; // Última fecha registrada

                // 4️ Obtener los cursos y horarios del facilitador por periodo.
                var cursosFacilitador = await _context.IdCursosXFacilitadorAsync(idUsuarioDoc, anioPeriodo);
                var horarios = await _context.HorariosXperiodoAsync(anioPeriodo, idUsuarioDoc);

                bool hayAccesosPosteriores = false; // Bandera para saber si hay accesos nuevos

                // 5️ Recorrer cada curso del facilitador.
                foreach (var curso in cursosFacilitador)
                {
                    int idCurso = curso.IdCurso; // Identificador del curso

                    // 6️ Obtener todos los accesos realizados por el facilitador en el curso especifico.
                    var accesos = await _context.ObtenerAccesosCursoEspecificoAsync(idUsuarioDoc, idCurso);

                    if (ultimaFechaIncidencia.HasValue)
                    {
                        // 7️ Si ya hubo incidencias antes, filtrar solo accesos posteriores a esa fecha + 2 minutos.
                        DateTime limiteConMargen = ultimaFechaIncidencia.Value.AddMinutes(2);

                        accesos = accesos
                            .Where(a => a.FechaHora > limiteConMargen) // Solo accesos más recientes
                            .OrderBy(a => a.FechaHora)                 // Orden cronológico
                            .ToList();

                        // Si no hay accesos nuevos, pasar al siguiente curso.
                        if (!accesos.Any())
                            continue;
                    }
                    else if (accesos.Any())
                    {
                        // Si no hay incidencias previas y sí hay accesos, marcar que existen accesos a revisar.
                        hayAccesosPosteriores = true;
                    }

                    // 8️ Analizar cada acceso del curso.
                    foreach (var acceso in accesos)
                    {
                        DateTime fechaAcceso = acceso.FechaHora;  // Fecha completa del acceso
                        TimeSpan horaAcceso = fechaAcceso.TimeOfDay; // Solo la hora del acceso

                        // Obtener el día de la semana en español (ej. "lunes", "martes", etc.)
                        string diaAcceso = fechaAcceso
                            .ToString("dddd", new System.Globalization.CultureInfo("es-ES"))
                            .ToLower();

                        // 9️ Buscar si ese acceso coincide con el horario programado del facilitador.
                        var cursoProgramado = horarios.FirstOrDefault(h =>
                        {
                            // Selecciona el campo de horario del día correspondiente.
                            string horasDia = diaAcceso switch
                            {
                                "lunes" => h.lunes,
                                "martes" => h.martes,
                                "miércoles" => h.miercoles,
                                "miercoles" => h.miercoles,
                                "jueves" => h.jueves,
                                "viernes" => h.viernes,
                                "sábado" => h.sabado,
                                "sabado" => h.sabado,
                                "domingo" => h.domingo,
                                _ => null
                            };

                            // Si no hay horario para ese día, no aplica.
                            if (string.IsNullOrWhiteSpace(horasDia))
                                return false;

                            // El formato esperado es "HH:mm-HH:mm"
                            var partes = horasDia.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            if (partes.Length != 2)
                                return false;

                            // Intenta convertir las horas a TimeSpan.
                            if (TimeSpan.TryParse(partes[0], out var inicio) && TimeSpan.TryParse(partes[1], out var fin))
                                // Devuelve true si la hora del acceso está dentro del rango del horario.
                                return horaAcceso >= inicio && horaAcceso <= fin;

                            return false;
                        });

                        // 10️ Si el acceso coincide con un horario, pero el curso no es el correcto → incidencia.
                        if (cursoProgramado != null && cursoProgramado.cve_EE != curso.cve_curso)
                        {
                            // Recupera la hora programada según el día.
                            string horaProgramada = diaAcceso switch
                            {
                                "lunes" => cursoProgramado.lunes,
                                "martes" => cursoProgramado.martes,
                                "miércoles" => cursoProgramado.miercoles,
                                "miercoles" => cursoProgramado.miercoles,
                                "jueves" => cursoProgramado.jueves,
                                "viernes" => cursoProgramado.viernes,
                                "sábado" => cursoProgramado.sabado,
                                "sabado" => cursoProgramado.sabado,
                                "domingo" => cursoProgramado.domingo,
                                _ => ""
                            };

                            // Incrementar contadores de incidencias
                            contadorIncidenciasFacilitador++;
                            resumen.TotalIncidencias++;

                            // Registrar el detalle de la incidencia detectada.
                            resumen.IncidenciasDetectadas.Add(new SetIncidenciasAccesoDetectadas
                            {
                                fk_Usuario = idUsuarioDoc,                             // Facilitador
                                cve_ProgramaEducativo = cursoProgramado.cve_PE,        // Programa educativo del curso programado
                                cve_Periodo = anioPeriodo,                             // Periodo académico
                                cve_EE_Programada = cursoProgramado.cve_EE,            // Experiencia educativa esperada
                                IdHorarioEE = cursoProgramado.id_infoAcademicoPE_EE,   // ID del horario
                                FechaIncidencia = fechaAcceso.ToString("yyyy-MM-dd"),  // Fecha de la incidencia
                                HoraProgramada = horaProgramada,                       // Horario donde debía estar
                                cve_EE_Accedida = curso.cve_curso,                     // Curso al que realmente accedió
                                fechaHoraAcceso = fechaAcceso.ToString("yyyy-MM-dd HH:mm") // Fecha y hora del acceso
                            });
                        }
                    }
                }

                // 11️ Si no se encontraron accesos nuevos, se omite el registro del facilitador.
                if (!hayAccesosPosteriores)
                    continue;

                // 12️ Si hubo incidencias, agregar un resumen por facilitador.
                if (contadorIncidenciasFacilitador > 0)
                {
                    resumen.Detalles.Add(new ResumenIncidenciaFacilitador
                    {
                        IdUsuarioDoc = idUsuarioDoc,
                        TotalIncidencias = contadorIncidenciasFacilitador
                    });
                }
            }

            // 13️ Si hay incidencias detectadas, se registran todas en lote en la base de datos.
            if (resumen.IncidenciasDetectadas.Any())
            {
                await _context.RegistrarIncidenciaAccesoLoteAsync(resumen.IncidenciasDetectadas);
            }

            // 14️ Se devuelve el resumen final, con totales y detalles.
            return resumen;
        }

        public string ObtenerHorarioPorDia(GetHorariosXPeriodo horario, string dia)
        {
            return dia switch
            {
                "lunes" => horario.lunes,
                "martes" => horario.martes,
                "miércoles" => horario.miercoles,
                "jueves" => horario.jueves,
                "viernes" => horario.viernes,
                "sábado" => horario.sabado,
                "domingo" => horario.domingo,
                _ => ""
            };
        }

    }
}
