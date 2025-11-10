using Newtonsoft.Json;

namespace moduloseguimiento.API.Models
{
    public class GetSPARHDataDecrypt
    {
        [JsonProperty("UserID")]
        public string? UserID { get; set; } //Pues ser usuario o num personal.

        [JsonProperty("sClaveAcceso")]
        public string? sClaveAcceso { get; set; }
    }
}
