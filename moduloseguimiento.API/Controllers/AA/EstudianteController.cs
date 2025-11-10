using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers.AA
{
    public class EstudianteController : Controller
    {

        private readonly ILogger<EstudianteController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IEncrypt _encrypt;
        private readonly IGetSPARHData _getSPARHData;

        public EstudianteController(ILogger<EstudianteController> logger, ApplicationDbContext dbContext, IEncrypt encrypt, IGetSPARHData getSPARHData)
        {
            _logger = logger;
            _dbContext = dbContext;
            _encrypt = encrypt;
            _getSPARHData = getSPARHData;
        }

        //Lista de Estudiantes X Experiencias Educativa con Filtros
        [HttpGet("ListaEstudiantes_X_EE")]
        public IActionResult ObtenerEstudiante(string idCurso, string idUsuarioDoc, int pageNumber = 1, int pageSize = 8, string? busquedaGeneral = null)
        {
            string salida;
            int estado;

            var resultado = _dbContext.ObtenerListaEstudiantes_X_EE(
                idCurso, idUsuarioDoc, pageNumber, pageSize, busquedaGeneral, out salida, out estado);

            if (estado != 200)
                return StatusCode(500, new { mensaje = salida });

            return Ok(resultado);
        }


        [HttpGet("DetallesEstudianteXCurso")]
        public ActionResult<List<GetDetallesEstudiante>> Detalles(int idCurso, string idUsuarioEstudiante)
        {
            try
            {
                var detalles = _dbContext.DetallesEstudianteXCurso(idCurso, idUsuarioEstudiante);

                if (detalles == null)
                    return NotFound("No se encontraron detalles de estudiante.");

                return Ok(detalles);
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
