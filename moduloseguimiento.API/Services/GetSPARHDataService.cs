using moduloseguimiento.API.Models;
using moduloseguimiento.API.Utilities;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using moduloseguimiento.API.Services.Interfaces;

namespace moduloseguimiento.API.Services
{
    public class GetSPARHDataService : IGetSPARHData
    {

        private const string _data = "ServicePublico";

        private readonly IEncrypt _encrypt;

        public GetSPARHDataService(ILogger<GetSPARHDataService> logger, IEncrypt encrypt)
        {
            _encrypt = encrypt;
        }

        public async Task<Respuesta> SPARHData(string usuario)
        {
            try
            {
                string aes = Utileria.GetAppSettingsValue("SPARH:AES");
                string clave = Utileria.GetAppSettingsValue("SPARH:LlaveCifrada");

                // Crear una instancia del modelo GetSPARHDataDecrypt
                var getSPARHDataDecrypt = new GetSPARHDataDecrypt
                {
                    UserID = usuario,
                    sClaveAcceso = clave
                };

                string ejemplo = _encrypt.SEncrypt(JsonConvert.SerializeObject(getSPARHDataDecrypt), aes);


                // Crear el JSON para el cuerpo
                var requestContent = new StringContent(JsonConvert.SerializeObject(ejemplo), Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {

                    //client.DefaultRequestHeaders.Add("ContentType", "application/json");

                    // Enviar la solicitud POST
                    var response = await client.PostAsync(_data, requestContent);

                    // Procesar la respuesta
                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();

                        // Deserializar dinámicamente
                        var result = JsonConvert.DeserializeObject<Respuesta>(responseContent);

                        //Filtracion de datos
                        //Si necesitas mas datos, puedes quitar esta filtracion.
                        if (result.Contenido is JObject contenidoObj)
                        {
                            var filteredContenido = new JObject
                            {
                                ["noper"] = contenidoObj["noper"],
                                ["nomb"] = contenidoObj["nomb"],
                                ["apat"] = contenidoObj["apat"],
                                ["amat"] = contenidoObj["amat"],
                                ["correo"] = contenidoObj["correo"],
                                ["cvelogin"] = contenidoObj["cvelogin"],
                                ["nzon"] = contenidoObj["nzon"],
                                ["dzon"] = contenidoObj["dzon"]
                            };

                            // Asignamos los datos filtrados al campo 'Contenido'
                            result.Contenido = filteredContenido;
                        }

                        return result;
                    }
                    else
                    {
                        // Manejar errores HTTP
                        throw new HttpRequestException($"Error al llamar al servicio: {response.StatusCode}");
                    }

                }
            }
            catch (Exception ex)
            {
                // Manejo general de excepciones
                throw new Exception("Error no esperado al contactar con el servidor SPARH", ex);
            }
        }

    }
}
