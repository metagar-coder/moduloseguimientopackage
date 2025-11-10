namespace moduloseguimiento.API.Services.Interfaces
{
    public interface IServiceEmail
    {
        Task EnviarEmail(List<string> emailsReceptores, string tema, string cuerpo);
    }
}
