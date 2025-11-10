using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services.Interfaces
{
    public interface IActiveDirectory
    {
        Task<bool> ValidarUsuarioAD(string userId, string pwd);
        //Task<ActiveDirectoryUser> GetInfoUsuarioAD(string userId);
        Task<Respuesta> GetInfoUsuarioAD(string userId);
    }
}
