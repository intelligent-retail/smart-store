using Newtonsoft.Json;

namespace ItemService.ItemMaster.Models
{
    public class Tax
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("rate")]
        public int Rate { get; set; }
        
        [JsonProperty("roundDigit")]
        public int RoundDigit { get; set; }
        
        [JsonProperty("roundMethod")]
        public string RoundMethod { get; set; }
        
        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }
        
        [JsonProperty("taxType")]
        public string TaxType { get; set; }
    }
}