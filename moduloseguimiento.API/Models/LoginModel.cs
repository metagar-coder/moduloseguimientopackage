using System.ComponentModel.DataAnnotations;

namespace moduloseguimiento.API.Models
{
    public class LoginModel
    {

        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class ValidarUsuarioAD
    {
        public bool isvalid { get; set; }

    }

    public class validUserAD
    {
        public bool isvalid { get; set; }

        public Usuario? DatosDelUsuario { get; set; }

        public TokenJWT? TokenJTW { get; set; }
    }


    public class TokenJWT
    {
        public string accessToken { get; set; }

        public string refreshToken { get; set; }
    }

}
