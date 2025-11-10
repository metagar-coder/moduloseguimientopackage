using Microsoft.AspNetCore.Mvc;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers.AA
{
    public class ExperienciaEducativaController : Controller
    {
        private readonly ILogger<ExperienciaEducativaController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;

        public ExperienciaEducativaController(ILogger<ExperienciaEducativaController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
        }

        [HttpGet("ListaCursos_X_CSeguimiento")]
        public IActionResult ObtenerMonitores(
        string monitorArea,
        int pageNumber = 1,
        int pageSize = 3,
        string? busquedaGeneral = null,
        string? programaEducativo = null,
        string? area = null,
        string? region = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ObtenerCursos_X_CSeguimientoFiltrado(
                monitorArea,
                pageNumber,
                pageSize,
                busquedaGeneral,
                programaEducativo,
                area,
                region,
                out salida,
                out estado);

            return estado switch
            {
                200 => Ok(resultado),
                400 => BadRequest(new { mensaje = salida}),
                404 => NotFound(new { mensaje = salida}),
                _ => StatusCode(500, new { mensaje = salida})
            };


        }


    }
}
