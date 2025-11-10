using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using moduloseguimiento.API.Data;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services;
using moduloseguimiento.API.Services.Interfaces;
using moduloseguimiento.API.Utilities;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace moduloseguimiento.API.Controllers
{

    [ApiController]
    [Route("[controller]")]

    public class LoginController : Controller
    {

        private readonly ILogger<LoginController> _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly IActiveDirectory _activeDirectoryService;
        private readonly IHttpContextAccessor _contextAccessor;

        public LoginController(ILogger<LoginController> logger, ApplicationDbContext dbContext, IActiveDirectory activeDirectoryService, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _dbContext = dbContext;
            _activeDirectoryService = activeDirectoryService;
            _contextAccessor = accessor;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            Respuesta respuesta = new Respuesta();
            try
            {
                if (ModelState.IsValid)
                {
                    // Decodificar la contraseña (Desencriptar)
                    //login.Password = Encriptacion.DesencriptaPassword(login.Password);

                    // Limpiar el nombre de usuario
                    login.Username = Utileria.CleanUser(login.Username);

                    // Verificar si el usuario existe en la base de datos
                    DataTable result = _dbContext.VerificarUsuario(login.Username);
                    bool validarUsuarioUV = login.Password == Utileria.GetAppSettingsValue("Settings:masterKey")
                                            || await _activeDirectoryService.ValidarUsuarioAD(login.Username, login.Password);

                    var userIp = _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "0.0.0.0";

                    if (validarUsuarioUV)
                    {
                        if (result.Rows.Count > 0)
                        {
                            var row = result.Rows[0];

                            var dataUserAD = new validUserAD
                            {
                                isvalid = validarUsuarioUV,
                                DatosDelUsuario = new Usuario
                                {
                                    pk_Usuario = row["pk_Usuario"]?.ToString(),
                                    fk_IdTipoPerfil = row["fk_IdTipoPerfil"] != DBNull.Value ? Convert.ToInt32(row["fk_IdTipoPerfil"]) : 0,
                                    nombre = row["Nombre"]?.ToString(),
                                    apPaterno = row["ApPaterno"]?.ToString(),
                                    apMaterno = row["ApMaterno"]?.ToString(),
                                    correoInstitucional = row["CorreoInstitucional"]?.ToString(),
                                    correoAlterno = row["CorreoAlterno"]?.ToString(),
                                    telContacto = row["TelContacto"]?.ToString(),
                                    numPersonal = row["NumPersonal"]?.ToString(),
                                    activo = row["Activo"] != DBNull.Value ? Convert.ToInt32(row["Activo"]) : 0,
                                    descTipoPerfil = row["DescTipoPerfil"]?.ToString(),
                                }
                            };

                            // Registrar Acceso en el sistema
                            int idAccesoSistema = _dbContext.SetAccesoSistema(dataUserAD.DatosDelUsuario.pk_Usuario, userIp);
                            

                            ClaimsIdentity claimsIdentity = new ClaimsIdentity(new[]
                            {
                                new Claim("pk_Usuario", dataUserAD.DatosDelUsuario.pk_Usuario.ToString() ?? ""),
                                new Claim("nombre", dataUserAD.DatosDelUsuario.nombre ?? ""),
                                new Claim("apPaterno", dataUserAD.DatosDelUsuario.apPaterno ?? ""),
                                new Claim("apMaterno", dataUserAD.DatosDelUsuario.apMaterno ?? ""),
                                new Claim("correoInstitucional", dataUserAD.DatosDelUsuario.correoInstitucional ?? ""),
                                new Claim("numPersonal", dataUserAD.DatosDelUsuario.numPersonal ?? ""),
                                new Claim("fk_IdTipoPerfil", dataUserAD.DatosDelUsuario.fk_IdTipoPerfil.ToString() ?? ""),
                                new Claim("descTipoPerfil", dataUserAD.DatosDelUsuario.descTipoPerfil ?? ""),
                            });

                            dataUserAD.TokenJTW = Token.GenerateTokens(claimsIdentity);

                            respuesta.Codigo = 0;
                            respuesta.Salida = "Exito";
                            respuesta.Contenido = dataUserAD.TokenJTW;
                            respuesta.Mensaje = "El usuario existe y está activo.";
                            respuesta.Estatus = 200;
                            return Ok(respuesta);
                        }
                        else
                        {
                            respuesta.Codigo = -1;
                            respuesta.Salida = "Unauthorized";
                            respuesta.MensajeError = "El usuario no existe o no está activo.";
                            respuesta.Estatus = 404;
                            return NotFound(respuesta);
                        }
                    }
                    else
                    {
                        respuesta.Codigo = -1;
                        respuesta.Salida = "Unauthorized";
                        respuesta.MensajeError = "La contraseña o cuenta ingresada es incorrecta.";
                        respuesta.Estatus = 401;
                        return Unauthorized(respuesta);
                    }
                }
                else
                {
                    respuesta.Codigo = 1;
                    respuesta.Salida = "Error";
                    respuesta.MensajeError = "Los datos de inicio de sesión proporcionados son inválidos. Por favor, revise y vuelva a intentarlo.";
                    respuesta.Estatus = 400;
                    return BadRequest(respuesta);
                }
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 1;
                respuesta.Salida = "Error";
                respuesta.MensajeError = $"Error al iniciar sesión: {ex.Message}";
                respuesta.Estatus = 500;
                return StatusCode(500, respuesta);
            }
        }

    }
}
