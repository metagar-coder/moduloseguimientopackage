using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Controllers.AA
{
    public class BitacoraController : Controller
    {

        private readonly ApplicationDbContext _dbContext;

        public BitacoraController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("AccesosEminus4_X_Facilitador")]
        public IActionResult ObtenerAccesosEminus4XFacilitador(
        string idUsuarioDoc,
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null)
        {
            try
            {
                var resultado = _dbContext.AccesosEminus4_X_Facilitador(
                    idUsuarioDoc,
                    pageNumber, pageSize,
                    fechaInicio, fechaFin
                );

                if (resultado == null || resultado.Items == null || !resultado.Items.Any())
                    return NoContent();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error en el servidor", detalle = ex.Message });
            }
        }


    }
}
