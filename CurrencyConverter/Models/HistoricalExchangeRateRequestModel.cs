using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Models
{
    public class HistoricalExchangeRateRequestModel
    {
        public string BaseCurrency { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        
        public int PageNumber { get; set; } = 1;
        
        public int Records { get; set; } = 5;
    }


    public class HistoricalExchangeRateResponseModel
    {
        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }
        [JsonPropertyName("base")]

        public string Base { get; set; }
        [JsonPropertyName("start_date")]

        public string Start_date { get; set; }
        [JsonPropertyName("end_date")]
        public string End_date { get; set; }
        [JsonPropertyName("rates")]
        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
    }

    

   

}
