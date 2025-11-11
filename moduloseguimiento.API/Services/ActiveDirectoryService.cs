using Microsoft.OpenApi.Services;
using moduloseguimiento.API.Models;
using moduloseguimiento.API.Services.Interfaces;
using moduloseguimiento.API.Utilities;
using System.DirectoryServices;
using System.Text.Json;
using System.Text;
using SearchResult = System.DirectoryServices.SearchResult;

namespace moduloseguimiento.API.Services
{
    public class ActiveDirectoryService : IActiveDirectory
    {

        private const string _validarUsuarioAD = "WebService";
        private const string _obtenerInfoUsuarioAD = "WebService";


        public async Task<bool> ValidarUsuarioAD(string userId, string pwd)
        {
            try
            {
                dynamic content = false;
                var httpClient = new HttpClient();
                var data = new { UserId = userId, Pwd = pwd };
                var json = JsonSerializer.Serialize(data);
                var body = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(_validarUsuarioAD, body);
                if (response.IsSuccessStatusCode)
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    content = JsonSerializer.Deserialize<ValidarUsuarioAD>(await response.Content.ReadAsStringAsync(), jsonOptions).isvalid;
                }
                return content;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al contactar con el servidor AD", ex);
            }
        }



        public async Task<Respuesta> GetInfoUsuarioAD(string userId)
        {
            try
            {
                using var httpClient = new HttpClient();

                // Construir body
                var data = new { UserId = userId };
                var json = JsonSerializer.Serialize(data);
                var body = new StringContent(json, Encoding.UTF8, "application/json");

                // Llamar al servicio externo
                var response = await httpClient.PostAsync(_obtenerInfoUsuarioAD, body);

                if (!response.IsSuccessStatusCode)
                {
                    return new Respuesta
                    {
                        Codigo = (int)response.StatusCode,
                        MensajeError = $"Error al llamar AD: {response.StatusCode}"
                    };
                }

                // Leer la respuesta
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var adUser = JsonSerializer.Deserialize<ActiveDirectoryUser>(responseContent, jsonOptions);

                if (adUser == null)
                {
                    return new Respuesta
                    {
                        Codigo = 500,
                        MensajeError = "No existe información de ese usuario."
                    };
                }

                // Mapear ActiveDirectoryUser -> InfoUsuarioAD
                var info = new InfoUsuarioAD
                {
                    noper = int.TryParse(adUser.NumPerson, out var noper) ? noper : 11111,
                    nomb = adUser.FirstName,
                    apat = adUser.LastName?.Split(' ').FirstOrDefault() ?? string.Empty,
                    amat = string.Join(" ", adUser.LastName?
                              .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                              .Skip(1) ?? Array.Empty<string>()),
                    correo = adUser.Email,
                    cvelogin = adUser.UserId,
                };

                return new Respuesta
                {
                    Codigo = 200,
                    Contenido = info
                };
            }
            catch (Exception ex)
            {
                return new Respuesta
                {
                    Codigo = 500,
                    MensajeError = $"Error al obtener datos desde AD: {ex.Message}"
                };
            }
        }

    }
}
