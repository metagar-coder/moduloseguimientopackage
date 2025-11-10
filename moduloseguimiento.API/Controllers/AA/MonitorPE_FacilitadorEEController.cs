using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers.AA
{
    [ApiController]
    [Route("[controller]")]

    public class MonitorPE_FacilitadorEEController : Controller
    {
        private readonly ILogger<MonitorPE_FacilitadorEEController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;

        public MonitorPE_FacilitadorEEController(ILogger<MonitorPE_FacilitadorEEController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
        }


        //Lista de MonitoresPE con los facilitadores y Experiencias Educativas a su cargo con Filtros
        [HttpGet("ListaMPE_FacEE")]
        public IActionResult ObtenerMonitores_FacilitadoresEE(string monitorArea, int pageNumber = 1, int pageSize = 3, string? busquedaGeneral = null, string? PE = null, string? region = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ObtenerMonitoresPE_FacEE_Filtrado(
                monitorArea, pageNumber, pageSize, busquedaGeneral, PE, region, out salida, out estado);

            if (estado != 200)
                return StatusCode(500, new { mensaje = salida });

            return Ok(resultado);
        }

        //Lista de Facilitadores_ExperienciasEducativas X MonitorPE.
        [HttpGet("ListaFacEE_X_MPE")]
        public IActionResult ListaFacEE_X_MonitorPE(string usuarioPE, string usuarioArea, string Cve_PE)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ObtenerFacEE_X_MonitorPE(
                usuarioPE, usuarioArea, Cve_PE, out salida, out estado);

            if (estado != 200)
                return StatusCode(500, new { mensaje = salida });

            return Ok(resultado);
        }


        // Inactivar la relación entre FacilitadorEE y su MonitorPE (Botón Eliminar)
        [HttpPost("DarBajaFacEE_MonitorPE")]
        public IActionResult DarBajaFacEE_MonitorPE([FromQuery] int idMonitorDoc)
        {
            var respuesta = new Respuesta();

            // Validación inicial
            if (idMonitorDoc <= 0)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Parámetro inválido";
                respuesta.MensajeError = "El parámetro 'idMonitorDoc' debe ser mayor que cero.";
                respuesta.Estatus = 400;
                return BadRequest(respuesta);
            }

            try
            {
                _dbContext.BajaRelacionFacEE_MonitorPE(idMonitorDoc, out string salida, out int estado);

                switch (estado)
                {
                    case 200:
                        respuesta.Codigo = 0;
                        respuesta.Salida = "Éxito";
                        respuesta.Mensaje = salida;
                        respuesta.Estatus = 200;
                        return Ok(respuesta);

                    case 404:
                        respuesta.Codigo = 1;
                        respuesta.Salida = "No encontrado";
                        respuesta.MensajeError = salida;
                        respuesta.Estatus = 404;
                        return NotFound(respuesta);

                    default:
                        respuesta.Codigo = 1;
                        respuesta.Salida = "Error desconocido";
                        respuesta.MensajeError = salida;
                        respuesta.Estatus = 500;
                        return StatusCode(500, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error interno";
                respuesta.MensajeError = $"Excepción al realizar la operación: {ex.Message}";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }
        }


        [HttpGet("CatalogoPE_X_MonitorArea")]
        public ActionResult<List<GetCatalogoPE_X_CS>> catalogoPEXMonitorArea(string monitorArea)
        {
            try
            {
                var PEs = _dbContext.CatalogoPE_X_CS(monitorArea);

                if (PEs == null)
                    return NotFound("No se encontraron Programas Educativos.");

                return Ok(PEs);
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


        [HttpGet("CatalogoAreasAcademicas")]
        public ActionResult<List<AreaAcademica>> catalogoAreaAcademica()
        {
            try
            {
                var Areas = _dbContext.CatalogoAreaAcademica();

                if (Areas == null)
                    return NotFound("No se encontraron Programas Educativos.");

                return Ok(Areas);
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

    }
}
