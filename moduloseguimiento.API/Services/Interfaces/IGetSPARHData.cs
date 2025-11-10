using moduloseguimiento.API.Models;

namespace moduloseguimiento.API.Services.Interfaces
{
    public interface IGetSPARHData
    {
        Task<Respuesta> SPARHData(string usuario);
    }
}
