namespace moduloseguimiento.API.Models
{
    public class Email
    {
        public List<string> EmailsReceptores { get; set; } = new List<string>();
        public string Tema { get; set; }
        public string Cuerpo { get; set; }
    }


    public class EmailEnviado
    {
        public string cve_PE {  get; set; }
        public int idCurso { get; set; }
        public string periodo { get; set; }
        public string asunto { get; set; }
        public string mensaje { get; set; }
        public string destinatarioId { get; set; }
        public string remitenteId { get; set; }
    }

    public class EmailsXFacilitador
    {
        public int idCorreoEnviado { get; set; }
        public string cve_PE { get; set; }
        public string programaEducativo { get; set; }
        public int idCurso { get; set; }
        public string curso { get; set; }
        public string cve_Periodo { get; set; }
        public string periodo { get; set; }
        public string asunto { get; set; }
        public string mensaje { get; set; }
        public string destinatarioId { get; set; }
        public string nombreDestinatario {  get; set; }
        public string remitenteId { get; set; }
        public string nombreRemitente { get; set; }
        public string fechaCorreoEnviado { get; set; }
    }

}
