namespace moduloseguimiento.API.Models
{
    public class ActiveDirectoryUser
    {
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string NoCampus { get; set; }
        public string NameCampus { get; set; }
        public string NumPerson { get; set; }
    }


    public class UsuarioADRequest
    {
        public string UserId { get; set; }
    }

    public class InfoUsuarioAD
    {
        public int noper {  get; set; }
        public string nomb {  get; set; }
        public string apat {  get; set; }
        public string amat { get; set; }
        public string correo { get; set; }
        public string cvelogin {  get; set; }
    }


}
