using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services;
using moduloseguimiento.API.Services.Interfaces;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace moduloseguimiento.API.Controllers.AA
{

    [ApiController]
    [Route("[controller]")]
    //[Authorize(Policy = "Administrador")] //[Authorize(Roles = ("Administrador"))]*/

    public class MonitorPEController : Controller
    {

        private readonly ILogger<MonitorPEController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;
        private readonly IActiveDirectory _activeDirectory;
        private readonly OracleService _oracleService;

        public MonitorPEController(ILogger<MonitorPEController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData, IActiveDirectory activeDirectory, OracleService oracleService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
            _activeDirectory = activeDirectory;
            _oracleService = oracleService;
        }

        //Lista de monitores con Filtros
        [HttpGet("ListaMonitoresPE")]
        public IActionResult ObtenerMonitores(string monitorArea, int pageNumber = 1, int pageSize = 3, string? busquedaGeneral = null, string? rol = null, string? dependencia = null, string? region = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ObtenerMonitoresPEFiltrado(
                monitorArea, pageNumber, pageSize, busquedaGeneral, rol, dependencia, region, out salida, out estado);

            if (estado != 200)
                return StatusCode(500, new { mensaje = salida });

            return Ok(resultado);
        }


        //Catalogo de Dependencias
        [HttpGet("ListaDependencias")]
        public ActionResult<List<Dependencia>> ListaDependencias(string monitorArea)
        {
            try
            {
                var dependencias = _dbContext.ListaDependencias(monitorArea, out string salida, out int estado);

                if (dependencias == null)
                    return NotFound("No se encontraron dependencias.");

                return Ok(dependencias);
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



        //Catalogo de Programa Educativos dependiendo la dependencia enviada.
        [HttpGet("ListaPE_X_Dependencia")]
        public ActionResult<List<ProgramaEducativo>> ListaPE_X_Dependencias(string dependencia, string monitorArea)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dependencia))
                {
                    return BadRequest(new
                    {
                        Codigo = 1,
                        Salida = "Error",
                        MensajeError = "El parámetro 'dependencia' no puede estar vacío.",
                        Estatus = 400
                    });
                }

                var programasEducativos = _dbContext.ListaPE_X_Dependencias(dependencia, monitorArea, out string salida, out int estado);

                if (programasEducativos == null || !programasEducativos.Any())
                {
                    return NotFound(new
                    {
                        Codigo = 0,
                        Salida = salida,
                        Estatus = 404
                    });
                }

                return Ok(programasEducativos);
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



        //Inactivar la relacion de un monitor PE con un programa Educativo (Es para el boton de eliminar)
        [HttpPost("DarBajaMonitorPE")]
        public IActionResult DarBajaMonitorPE([FromBody] EliminarMonitorPE inactivar)
        {
            var respuesta = new Respuesta();

            try
            {
                if (string.IsNullOrWhiteSpace(inactivar.usuario) || inactivar.IdPEDependencia <= 0)
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error de validación";
                    respuesta.MensajeError = "El campo 'usuario' no puede estar vacío y 'IdPEDependencia' debe ser mayor que 0.";
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }

                string salida;
                int estado;

                _dbContext.BajaMonitorPE(inactivar.usuario, inactivar.IdPEDependencia, out salida, out estado);

                if (estado == 200)
                {
                    respuesta.Codigo = 0;
                    respuesta.Salida = "Éxito";
                    respuesta.Mensaje = salida;
                    respuesta.Estatus = 200;
                    return Ok(respuesta);
                }
                else
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 404; // Podría ser 400 si el error fue por datos inválidos, ajusta según lógica de tu SP
                    return NotFound(respuesta);
                }
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


        //Registrar una nueva relacion de un MonitorPE a un Programa educativo (Activo) y desactivar la relacion actual (Inactivo)
        [HttpPost("ActualizarMonitorPE")]
        public IActionResult ActualizarMonitorPE([FromBody] ActualizarMonitorPE actualizar)
        {
            var respuesta = new Respuesta();

            try
            {
                // Validaciones de entrada
                if (string.IsNullOrWhiteSpace(actualizar.usuario) ||
                    string.IsNullOrWhiteSpace(actualizar.Cve_dependencia) ||
                    string.IsNullOrWhiteSpace(actualizar.programaEducativo) ||
                    actualizar.idPEDependencia <= 0)
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error de validación";
                    respuesta.MensajeError = "Todos los campos son requeridos y deben contener valores válidos.";
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }

                // Llamada a procedimiento almacenado
                _dbContext.actualizarMonitorPE(actualizar.usuario, actualizar.Cve_dependencia, actualizar.programaEducativo, actualizar.idPEDependencia, out string salida, out int estado);

                if (estado == 200)
                {
                    respuesta.Codigo = 0;
                    respuesta.Salida = "Éxito";
                    respuesta.Mensaje = salida;
                    respuesta.Estatus = 200;
                    return Ok(respuesta);
                }
                else if (estado == 404)
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 404;
                    return NotFound(respuesta);
                }
                else
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 409; // Puede cambiarse a 400 si el SP lo indica
                    return StatusCode(409, respuesta);
                }
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


        //Registrar un nuevo MonitorPE con un programa educativo (Boton de Agregar)
        [HttpPost("NuevoMonitorPE")]
        public IActionResult NuevoMonitorPE([FromBody] NewMonitorPE monitorPE)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                var respuestaError = new Respuesta
                {
                    Codigo = -1,
                    Salida = "Error de validación",
                    MensajeError = string.Join(" | ", errores),
                    Estatus = 400
                };

                return BadRequest(respuestaError);
            }

            Respuesta respuesta = new Respuesta();
            try
            {
                string salida;
                int estado;

                _dbContext.RegistrarMonitorPE(monitorPE, out salida, out estado);

                if (estado == 200)
                {
                    respuesta.Codigo = 0;
                    respuesta.Salida = "Éxito";
                    respuesta.Mensaje = salida;
                    respuesta.Estatus = 200;
                    return Ok(respuesta);
                }
                else if (estado == 409)
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Usuario ya registrado.";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 409;
                    return Conflict(respuesta);
                }
                else if (estado == 400)
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Error de validación";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }
                else
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida ?? "Estado inesperado";
                    respuesta.Estatus = estado;
                    return StatusCode(estado, respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 1;
                respuesta.Salida = "Error";
                respuesta.MensajeError = $"Error al realizar la operación: {ex.Message}";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }
        }


        //*********************************************************************************************************************************

        [HttpPost]
        [Route("GetSPARHData")]
        public async Task<IActionResult> ObtenerDatosSPARH(string usuario)
        {
            try
            {
                // Llamar al servicio para obtener los datos
                var respuestaDTO = await _getSPARHData.SPARHData(usuario);

                // Serializar la respuestaDTO para devolverla al cliente
                var jsonResponse = JsonConvert.SerializeObject(respuestaDTO);

                return Ok(jsonResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("GetUsuarioAD")]
        public async Task<IActionResult> ObtenerInfoUsuarioAD([FromBody] UsuarioADRequest request)
        {
            try
            {
                // Llamar al servicio para obtener los datos
                var respuestaDTO = await _activeDirectory.GetInfoUsuarioAD(request.UserId);

                if (respuestaDTO == null)
                    return NotFound("No se encontraron datos del usuario.");

                return Ok(respuestaDTO); // ya regresas JSON directamente
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }


        //*********************************************************************************************************************************

        [HttpGet("ListaEE_X_Facilitador")]
        public ActionResult<List<GetListaEE_X_facilitador>> ListaEE(string monitorDoc, string Cve_PE, string Cve_Dependencia)
        {
            try
            {
                var ExperienciasEducativas = _dbContext.ListaEE_X_Facilitador(monitorDoc, Cve_PE, Cve_Dependencia, out string salida, out int estado);

                if (ExperienciasEducativas == null)
                    return NotFound("No se encontraron dependencias.");

                return Ok(ExperienciasEducativas);
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

        // Registrar una nueva relación de un MonitorPE con una experiencia educativa.
        // Le da prioridad a la relacion del facilitador con MonitorPE y solo muestra el mensaje de esa operacion, sin importar la salida de los horarios.
        [HttpPost("RegistrarRelacionMonitorPE_EE")]
        public async Task<IActionResult> RelacionMonitorPE_EE([FromBody] NewMonitorPE_EE monitorPE_EE)
        {
            if (monitorPE_EE == null || !ModelState.IsValid)
                return BadRequest("El cuerpo de la solicitud es inválido.");

            try
            {
                // 1. Registrar la relación en la BD (PRIORIDAD)
                string salida;
                int estado;
                _dbContext.RelacionMonitorPE_EE(monitorPE_EE, out salida, out estado);

                if (estado != 200)
                {
                    return StatusCode(estado, new Respuesta
                    {
                        Codigo = 1,
                        Salida = "Error",
                        MensajeError = salida
                    });
                }

                // 2. Intentar registrar los horarios, pero sin afectar la respuesta
                try
                {
                    await _oracleService.RegistrarHorariosFacilitadorAsync(monitorPE_EE.usuarioDoc);
                }
                catch
                {
                    // Ignorar errores de horarios completamente
                }

                // 3. La respuesta final SOLO refleja lo de la BD
                return Ok(new Respuesta
                {
                    Codigo = 0,
                    Salida = "Éxito",
                    Mensaje = salida // mensaje que devuelve la BD
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new Respuesta
                {
                    Codigo = 1,
                    Salida = "Error",
                    MensajeError = $"Ocurrió un error inesperado: {ex.Message}"
                });
            }
        }


    }
}
