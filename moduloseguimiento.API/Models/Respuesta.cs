namespace moduloseguimiento.API.Models
{
    public class Respuesta
    {

        public int? Codigo { get; set; }

        public string? Salida { get; set; }

        public object? Contenido { get; set; }

        public int? Dato { get; set; } // Aqui recibimos los id´s importantes para crear, actualizar o eliminar.

        public string? Mensaje { get; set; }

        public string? MensajeError { get; set; }

        public int? Estatus { get; set; }
    }
}
