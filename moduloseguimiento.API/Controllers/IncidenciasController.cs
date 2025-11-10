using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Policy = "Administrador")] //[Authorize(Roles = ("Administrador"))]*/
    public class IncidenciasController : Controller
    {

        private readonly ILogger<IncidenciasController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;

        public IncidenciasController(ILogger<IncidenciasController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
        }

     


        // ************************************************* LISTA DE INCIDENCIAS ********************************************************************
        [HttpGet("ListaIncidenciasActividadesXEE")]
        public async Task<IActionResult> IncidenciasActividadesXCurso(
        int idCurso,
        int pageNumber = 1,
        int pageSize = 10,
        string? busquedaGeneral = null)
        {
            try
            {
                var actividadesRevisadas = await _dbContext.ListaIncidenciasActividadesXCursoAsyncPaginacion(
                    idCurso,
                    pageNumber,
                    pageSize,
                    busquedaGeneral
                );

                if (actividadesRevisadas == null || !actividadesRevisadas.Items.Any())
                    return NotFound("No hay incidencias registradas en este curso.");

                return Ok(actividadesRevisadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Codigo = 1,
                    Salida = "Error",
                    MensajeError = $"Error al obtener información: {ex.Message}",
                    Estatus = 500
                });
            }
        }


        [HttpGet("ListaIncidenciasForosXEE")]
        public async Task<IActionResult> IncidenciasForosXCurso(
        int idCurso,
        int pageNumber = 1,
        int pageSize = 10,
        string? busquedaGeneral = null)
        {
            try
            {
                var actividadesRevisadas = await _dbContext.ListaIncidenciasForosXCursoAsyncPaginacion(
                    idCurso,
                    pageNumber,
                    pageSize,
                    busquedaGeneral
                );

                if (actividadesRevisadas == null || !actividadesRevisadas.Items.Any())
                    return NotFound("No hay incidencias registradas en este curso.");

                return Ok(actividadesRevisadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Codigo = 1,
                    Salida = "Error",
                    MensajeError = $"Error al obtener información: {ex.Message}",
                    Estatus = 500
                });
            }
        }


        [HttpGet("ListaIncidenciasAsistenciaXEE")]
        public async Task<IActionResult> IncidenciasAsistenciasXCurso(
        int idCurso,
        int pageNumber = 1,
        int pageSize = 10,
        string? busquedaGeneral = null)
        {
            try
            {
                var actividadesRevisadas = await _dbContext.ListaIncidenciasAsistenciasXCursoAsyncPaginacion(
                    idCurso,
                    pageNumber,
                    pageSize,
                    busquedaGeneral
                );

                if (actividadesRevisadas == null || !actividadesRevisadas.Items.Any())
                    return NotFound("No hay incidencias registradas en este curso.");

                return Ok(actividadesRevisadas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Codigo = 1,
                    Salida = "Error",
                    MensajeError = $"Error al obtener información: {ex.Message}",
                    Estatus = 500
                });
            }
        }


        [HttpPost("IncidenciasRevisadas")]
        public async Task<IActionResult> MarcarIncidenciasRevisadas(int idCurso)
        {
            try
            {
                var (salida, estado) = await _dbContext.MarcarIncidenciasRevisadasAsync(idCurso);

                // Devolver código HTTP según el estado del SP
                if (estado == 200)
                    return Ok(new { mensaje = salida, estado });
                else
                    return StatusCode(500, new { mensaje = salida, estado });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error ejecutando el SP: {ex.Message}", estado = -1 });
            }
        }

    }
}
