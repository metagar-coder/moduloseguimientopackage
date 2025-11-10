using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Policy = "")]
    public class EmailsController : Controller
    {
        private readonly IServiceEmail servicioEmail;
        private readonly ApplicationDbContext _dbContext;

        public EmailsController(IServiceEmail servicioEmail, ApplicationDbContext dbContext)
        {
            this.servicioEmail = servicioEmail;
            _dbContext = dbContext;
        }

        [HttpPost("EnvioEmail")]
        public async Task<ActionResult> EnviarEmail([FromBody] Email request)
        {
            Respuesta respuesta = new Respuesta();

        
            // Validacion de datos de entrada
            if (request.EmailsReceptores == null || !request.EmailsReceptores.Any())
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error de validación";
                respuesta.MensajeError = "Debe proporcionar al menos un destinatario.";
                respuesta.Estatus = 400;
                return BadRequest(respuesta);
            }

            if (string.IsNullOrWhiteSpace(request.Tema))
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error de validación";
                respuesta.MensajeError = "El tema del correo es obligatorio.";
                respuesta.Estatus = 400;
                return BadRequest(respuesta);
            }

            if (string.IsNullOrWhiteSpace(request.Cuerpo))
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error de validación";
                respuesta.MensajeError = "El cuerpo del correo es obligatorio.";
                respuesta.Estatus = 400;
                return BadRequest(respuesta);
            }


            try
            {
                // Llamada al servicio para enviar el correo
                await servicioEmail.EnviarEmail(request.EmailsReceptores, request.Tema, request.Cuerpo);

                respuesta.Codigo = 1;
                respuesta.Salida = "Éxito";
                respuesta.Mensaje = "Correo enviado correctamente a todos los destinatarios.";
                respuesta.Estatus = 200;
                return Ok(respuesta);
            }
            catch (FormatException)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error de validación";
                respuesta.MensajeError = "Uno o más destinatarios tienen un formato de correo inválido.";
                respuesta.Estatus = 400;
                return BadRequest(respuesta);
            }
            catch (InvalidOperationException)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error de configuración";
                respuesta.MensajeError = "Ocurrió un problema al configurar el servicio de correo.";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }
            catch (Exception)
            {
                respuesta.Codigo = -1;
                respuesta.Salida = "Error inesperado";
                respuesta.MensajeError = "Ocurrió un error inesperado al enviar el correo. Por favor, inténtelo nuevamente más tarde.";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }


        }


        [HttpPost("RegistrarCorreoEnviado")]
        public async Task<IActionResult> RegistrarCorreoEnviado([FromBody] EmailEnviado registro)
        {
            var respuesta = new Respuesta();

            try
            {
                // Validar si el JSON está completamente vacío {}
                if (registro == null)
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error de validación";
                    respuesta.MensajeError = "No se pueden enviar campos nulos";
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }

                // Lógica para registrar el correo en la base de datos
                var (salida, estatusHTTP) = await _dbContext.RegistrarCorreoEnviadoAsync(registro);

                if (estatusHTTP == 200) // Éxito
                {
                    respuesta.Codigo = 0;
                    respuesta.Salida = "Éxito";
                    respuesta.Mensaje = salida;
                    respuesta.Estatus = 200;
                    return Ok(respuesta);
                }
                else if (estatusHTTP == 404) // No encontrado
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 404;
                    return NotFound(respuesta);
                }
                else // Otro error controlado
                {
                    respuesta.Codigo = -1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = salida;
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
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
        

        [HttpGet("CorreosEnviados_X_Facilitador")]
        public IActionResult ObtenerCorreosEnviadosXFacilitador(
        string periodo,
        string idUsuarioDoc,
        int pageNumber = 1,
        int pageSize = 5,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.CorreoEnviados_X_Periodo_Facilitador(
                periodo, idUsuarioDoc,
                pageNumber, pageSize,
                fechaInicio, fechaFin,
                out salida, out estado);

            return estado switch
            {
                200 => Ok(resultado),
                204 => BadRequest(new { mensaje = salida }),
                400 => BadRequest(new { mensaje = salida }),
                _ => StatusCode(500, new { mensaje = salida })
            };
        }
        
    }
}
