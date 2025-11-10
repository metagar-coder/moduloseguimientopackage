using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using System.Threading;

namespace moduloseguimiento.API.Controllers.AA
{
    [ApiController]
    [Route("[controller]")]

    public class ConfiguracionPEController : Controller
    {
        private readonly ILogger<ConfiguracionPEController> logger;
        private readonly ApplicationDbContext _dbContext;

        public ConfiguracionPEController(ILogger<ConfiguracionPEController> logger, ApplicationDbContext dbContext)
        {
            this.logger = logger;
            _dbContext = dbContext;
        }

        [HttpPost("Registrar_O_Act_ConfiguracionPE")]
        public IActionResult NuevaConfiguracionPE([FromBody] ConfiguracionProgramaEducativo configuracionPE)
        {
            var respuesta = new Respuesta();

            // Validación de modelo
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                respuesta.Codigo = -1;
                respuesta.Salida = "Error de validación";
                respuesta.MensajeError = string.Join(" | ", errores);
                respuesta.Estatus = 400;

                return BadRequest(respuesta);
            }

            try
            {
                string salida;
                int estado;

                _dbContext.RegistrarConfiguracionPE(configuracionPE, out salida, out estado);

                respuesta.Salida = estado == 200 ? "Éxito" : "Error";
                respuesta.Mensaje = estado == 200 ? salida : null;
                respuesta.MensajeError = estado != 200 ? salida : null;
                respuesta.Codigo = estado == 200 ? 0 : 1;
                respuesta.Estatus = estado;

                return estado switch
                {
                    200 => Ok(respuesta),
                    400 => BadRequest(respuesta),
                    404 => NotFound(respuesta),
                    _ => StatusCode(estado, respuesta)
                };
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Excepción";
                respuesta.MensajeError = $"Error al realizar la operación: {ex.Message}";
                respuesta.Estatus = 500;

                return StatusCode(500, respuesta);
            }
        }

       


        [HttpGet("ListaConfiguracionesPE")]
        public ActionResult<List<GetConfiguracionPE>> ListaConfiguracionesPE(string usuarioArea, string? Cve_PE = null, string? Cve_Dependencia = null, string? Periodo = null)
        {
            try
            {
                var configuraciones = _dbContext.ListaConfiguracionesPE(usuarioArea, Cve_PE, Cve_Dependencia, Periodo, out string salida, out int estado);

                if (configuraciones == null)
                    return NotFound("No se encontraron configuraciones.");

                return Ok(configuraciones);
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


        // Endpoint para eliminar una configuracion de un programa especifico.
        [HttpPost("EliminarConfiguracionPE")]
        public IActionResult EliminarConfiguracionPE(int idPEConf)
        {
            var respuesta = new Respuesta();

            try
            {
                if (idPEConf <= 0)
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error de validación";
                    respuesta.MensajeError = "El valor de 'idPEConf' debe ser mayor que 0.";
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }

                string salida;
                int estado;

                _dbContext.EliminarConfiguracionPE(idPEConf, out salida, out estado);

                respuesta.Salida = estado == 200 ? "Éxito" : "Error";
                respuesta.Codigo = estado == 200 ? 0 : 1;
                respuesta.Mensaje = estado == 200 ? salida : null;
                respuesta.MensajeError = estado != 200 ? salida : null;
                respuesta.Estatus = estado;

                return StatusCode(estado, respuesta);
            }
            catch (Exception ex)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error interno";
                respuesta.MensajeError = $"Error al realizar la operación: {ex.Message}";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }
        }


        //Endpoint para recuperar la lista de programas educativos que le corresponde a un monitor de Area especifico, con paginacion.
        [HttpGet("CatalogoPE_X_MA_ConPaginacion")]
        public ActionResult<List<GetCatalogoPExCSConPaginacion>> catalogoPEXMA_ConPaginacion(string monitorArea, int pageNumber = 1, int pageSize = 5, string? busquedaGeneral = null)
        {
            try
            {
                var PEs = _dbContext.CatalogoPExCSConPaginacion(monitorArea, pageNumber, pageSize, busquedaGeneral);

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


    }
}

