using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace rinha_backend.Models
{
    public class Transacao
    {
        [JsonPropertyName("valor")]
        public int Valor { get; set; }
        [JsonPropertyName("tipo")]
        public char Tipo { get; set; }
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; }
        [JsonIgnore]
        public DateTime RealizadoEm { get; set; }
    }
}
