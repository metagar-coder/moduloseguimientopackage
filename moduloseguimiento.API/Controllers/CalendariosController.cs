using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Services;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Policy = "Administrador")] //[Authorize(Roles = ("Administrador"))]*/
    public class CalendariosController : Controller
    {

        private readonly ILogger<CalendariosController> _logger;
        private readonly ICalendar _calendar;
        private readonly ApplicationDbContext _dbContext;

        public CalendariosController(ILogger<CalendariosController> _logger, ICalendar calendar, ApplicationDbContext dbContext)
        {
            _logger = _logger;
            _calendar = calendar;
            _dbContext = dbContext;
        }

  
        [HttpGet("RegistroDiasDescandoUV")]
        public async Task<IActionResult> RegistroDiasDescandoCalendarioUV([FromQuery] string idCalendario, [FromQuery] string periodo, [FromQuery] int idTipoCalendario)
        {
            var datos = await _calendar.RegistrarDiasDescanso(idCalendario, periodo, idTipoCalendario);
            return Ok(datos);
        }

        [HttpGet("DiasDescandoBD_CalendarioUV")]
        public async Task<IActionResult> ObtenerDiasDescansoCalendarioUV([FromQuery] string periodo)
        {
            var datos = await _dbContext.DiasDescansoCalendarioUV_X_PeriodoAsync(periodo);
            return Ok(datos);
        }

    }
}
