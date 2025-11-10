using moduloseguimiento.API.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace moduloseguimiento.API.Services
{
    public class EmailService : IServiceEmail
    {

        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task EnviarEmail(List<string> emailsReceptores, string tema, string cuerpo)
        {
            var emailEmisor = _configuration.GetValue<string>("EmailSettings:UserNameSMTP");
            var password = _configuration.GetValue<string>("EmailSettings:PasswordSMTP");
            var host = _configuration.GetValue<string>("EmailSettings:Domain");
            var puerto = _configuration.GetValue<int>("EmailSettings:Port");


            using var smtpCliente = new SmtpClient(host, puerto)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(emailEmisor, password)
            };

            var cuerpoFinal = "<p>" + cuerpo + "</p><p>Saludos cordiales,<br>Atentamente<br><b>Lis de Veracruz: Arte, Ciencia, Luz</b></p>      <hr>      <p><i>Este es un mensaje automático enviado por el SISTEMA DE EMINUS SEGUIMIENTO y no es necesario responder.</p>";

            var mensaje = new MailMessage
            {
                From = new MailAddress(emailEmisor),
                Subject = tema,
                Body = cuerpoFinal,
                IsBodyHtml = true
            };

            //Agregar cada correo en la lista de destinatarios
            foreach (var emailReceptor in emailsReceptores)
            {
                mensaje.To.Add(emailReceptor);
            }

            try
            {
                await smtpCliente.SendMailAsync(mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
            }



        }

    }
}
