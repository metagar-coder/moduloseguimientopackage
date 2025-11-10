namespace moduloseguimiento.API.Models
{
    public class Usuario
    {

        public string? pk_Usuario { get; set; }

        public int fk_IdTipoPerfil { get; set; }

        public string nombre { get; set; }

        public string apPaterno { get; set; }

        public string apMaterno { get; set; }

        public string correoInstitucional { get; set; }

        public string correoAlterno { get; set; }

        public string telContacto { get; set; }

        public string numPersonal { get; set; }

        public int activo { get; set; }

        public string descTipoPerfil { get; set; }

    }
}

