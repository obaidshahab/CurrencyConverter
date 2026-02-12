using System.Text.Json.Serialization;

namespace CurrencyConverter.Models
{

    public class CurrencyAPIModel
    {
        [JsonPropertyName("amount")]
        public float Amount { get; set; }
        [JsonPropertyName("base")]
        public string Base { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("rates")]
        public Dictionary<string,decimal> Rates { get; set; }
    }

    

}
