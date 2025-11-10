using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers.AA
{
    [ApiController]
    [Route("[controller]")]
    //[Authorize(Policy = "Administrador")] //[Authorize(Roles = ("Administrador"))]*/
    public class CargaAcademicaController : Controller
    {

        private readonly ILogger<CargaAcademicaController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;

        public CargaAcademicaController(ILogger<CargaAcademicaController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
        }


        [HttpGet("CatalogoPeriodos")]
        public ActionResult<List<Periodo>> catalogoPeriodos()
        {
            try
            {
                var Periodos = _dbContext.CatalogoPeriodos();

                if (Periodos == null)
                    return NotFound("No se encontraron Programas Educativos.");

                return Ok(Periodos);
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


        //Muestra la lista facilitadores Por Programa educativo, periodo y que esten monitoreados por su coordinador de seguimiento
        [HttpGet("FacilitadoresXPeriodo")]
        public IActionResult facilitadoresXPE_Periodo(
        string monitorArea,
        string Periodo)
        {
            string salida;
            int estado;

            var resultado = _dbContext.FacilitadoresXPeriodo(
                monitorArea,
                Periodo,
                out salida,
                out estado);

            return estado switch
            {
                200 => Ok(resultado),
                204 => BadRequest(new { mensaje = salida }),
                400 => BadRequest(new { mensaje = salida }),
                _ => StatusCode(500, new { mensaje = salida })
            };

        }


        //Muestra la lista de programa educativos que pertenece un facilitador.
        [HttpGet("ProgramasEducativosXFacilitador")]
        public IActionResult ProgramasEducativosXFacilitador(
        string Periodo,
        string idUsuarioDoc)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ProgramasEducativosXFacilitador(
                Periodo,
                idUsuarioDoc,
                out salida,
                out estado);

            return estado switch
            {
                200 => Ok(resultado),
                204 => BadRequest(new { mensaje = salida }),
                400 => BadRequest(new { mensaje = salida }),
                _ => StatusCode(500, new { mensaje = salida })
            };


        }


        // Experiencias Educativas (EE) X Programa Educativo (PE), Periodo y Docente
        [HttpGet("EE_X_PE_Periodo_Docente")]
        public IActionResult ExpEduXPE_Periodo_Docente(
        string IdUsuarioDoc,
        string Cve_PE,
        string Periodo)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ExperienciaEducativasXPE_Periodo_Docente(
                IdUsuarioDoc,
                Cve_PE,
                Periodo,
                out salida,
                out estado);

            return estado switch
            {
                200 => Ok(resultado),
                204 => BadRequest(new { mensaje = salida }),
                400 => BadRequest(new { mensaje = salida }),
                _ => StatusCode(500, new { mensaje = salida })
            };


        }


        [HttpGet("HorarioEE_X_PE_Periodo_Docente")]
        public IActionResult ExpEduXPE_Periodo_Docente(
        string ExperienciaEducativa,
        string Cve_EE,
        string Cve_PE,
        string Periodo,
        string IdUsuarioDoc)
        {
            string salida;
            int estado;

            var resultado = _dbContext.HorarioExperienciaEducativa(
                ExperienciaEducativa,
                Cve_EE,
                Cve_PE,
                Periodo,
                IdUsuarioDoc,
                out salida,
                out estado);

            return estado switch
            {
                200 => Ok(resultado),
                204 => BadRequest(new { mensaje = salida }),
                400 => BadRequest(new { mensaje = salida }),
                _ => StatusCode(500, new { mensaje = salida })
            };


        }


        [HttpGet("IncidenciasAcceso_X_Facilitador")]
        public IActionResult ObtenerIncidenciasFiltradas(
        string periodo,
        string idUsuarioDoc,
        int pageNumber = 1,
        int pageSize = 10,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.IncidenciasAcceso_X_Periodo_Facilitador(
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
