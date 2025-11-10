using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services;
using moduloseguimiento.API.Services.Interfaces;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace moduloseguimiento.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OracleTestController : ControllerBase
    {
        private readonly OracleService _oracleService;

        private readonly SeguridadSIIUService _seguridadService;

        public OracleTestController(OracleService oracleService, SeguridadSIIUService seguridadService)
        {
            _oracleService = oracleService;
            _seguridadService = seguridadService;
        }


        #region Endpoints_Oracle_Produccion
        [HttpGet("ProbarConexion_Produccion")]
        public IActionResult ProbarConexionProduccion()
        {
            OracleConnection conexion = null;

            try
            {
                conexion = _seguridadService.ConectaSIIU("segSWV_VEMI");

                if (conexion == null || conexion.State != ConnectionState.Open)
                {
                    return Unauthorized("❌ No se pudo conectar a la BD del SIIU o no tiene permisos.");
                }

                Console.WriteLine("✅ Conexión abierta correctamente en ProbarConexion");

                using var cmd = new OracleCommand("SELECT SYSDATE AS FECHA FROM DUAL", conexion);
                using var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return Ok(new
                    {
                        Mensaje = "✅ Conexión exitosa a Oracle",
                        FechaServidor = reader["FECHA"].ToString()
                    });
                }

                return Ok(new { Mensaje = "✅ Conexión exitosa, pero no devolvió resultados." });

            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ProbarConexion: {ex}");
                return StatusCode(500, $"Error al probar conexión: {ex.Message}");
            }
            finally
            {
                if (conexion != null)
                {
                    _seguridadService.DesconectaSIIU(conexion);
                    Console.WriteLine("🔌 Conexión cerrada.");
                }
            }
        }

        [HttpGet("Horario_Facilitador_Oracle_Produccion")]
        public async Task<IActionResult> HorarioProduccion(string idUsuarioDoc)
        {
            try
            {
                ResultadoHorarios horario = await _oracleService.Horario_Facilitador_Oracle_Produccion(idUsuarioDoc);
                return Ok(horario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        #endregion



        #region Endpoints_Oracle_Desarrollo/Produccion
        [HttpGet("Horarios_FacilitadorDTO")]
        public async Task<IActionResult> HorarioFacilitador(string idUsuarioDoc)
        {

            // ---------------------------------------- ORACLE - DESARROLLO ---------------------------------------------------------------------------

            // Traes los datos crudos de Oracle_Desarrollo
            //List<HorarioEE_Oracle> horario = await _oracleService.Horario_Facilitador_Oracle(idUsuarioDoc);

            // Mapea los datos (Oracle_Desarrollo) a mi modelo DTO para su insercion a SQL Server.
            //var dtoList = horario.Select(_oracleService.HorariosFacilitador).ToList();


            // ---------------------------------------- ORACLE - PRODUCCION ---------------------------------------------------------------------------

            // Traes los datos crudos de Oracle_Produccion
            ResultadoHorarios horario = await _oracleService.Horario_Facilitador_Oracle_Produccion(idUsuarioDoc);

            // Mapea los datos (Oracle_Desarrollo) a mi modelo DTO para su insercion a SQL Server.
            var dtoList = horario.Horarios.Select(_oracleService.HorariosFacilitador).ToList();

            return Ok(dtoList);
        }

        // Este endpoint registra los horarios de un solo facilitador
        [HttpPost("RegistrarHorarios_Facilitador")]
        public async Task<IActionResult> RegistrarHorarioFacilitador(string idUsuarioDoc)
        {
            var mensajes = await _oracleService.RegistrarHorariosFacilitadorAsync(idUsuarioDoc);

            return Ok(new { Mensajes = mensajes });
        }
        #endregion


        [HttpPost("ActualizarHorariosFacilitadores")]
        public async Task<IActionResult> ActualizarHorarios(string monitorArea)
        {
            if (string.IsNullOrWhiteSpace(monitorArea))
                return BadRequest("El parámetro 'monitorArea' es requerido.");

            int resultado = await _oracleService.ActualizarHorariosFacilitadoresAsync(monitorArea);

            // Responder según el código de salida
            switch (resultado)
            {
                case 1:
                    return Ok("✅ Sincronización realizada correctamente.");
                case 2:
                    return Ok("⚠️ La sincronización se realizó parcialmente: algunos facilitadores se sincronizaron, otros no.");
                case 3:
                    return Ok("❌ No fue posible sincronizar los horarios de los facilitadores.");
                case 0:
                case 4:
                    return Ok("⛔ No se encontraron facilitadores para sincronizar.");
                default:
                    return StatusCode(500, "⚠️ Error desconocido al intentar sincronizar los horarios de los facilitadores.");
            }


        }


    }
}
