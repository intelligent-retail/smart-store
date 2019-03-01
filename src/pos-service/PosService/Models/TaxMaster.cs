using Newtonsoft.Json;

namespace PosService.Models
{
    /// <summary>
    /// 税マスター
    /// </summary>
    public class TaxMaster
    {
        /// <summary>税コード</summary>
        [JsonProperty(PropertyName = "taxCode")]
        public string TaxCode { get; set; }

        /// <summary>税種別</summary>
        [JsonProperty(PropertyName = "taxType")]
        public string TaxType { get; set; }

        /// <summary>説明</summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>税率</summary>
        [JsonProperty(PropertyName = "rate")]
        public decimal Rate { get; set; }

        /// <summary>丸め桁</summary>
        [JsonProperty(PropertyName = "roundDigit")]
        public int RoundDigit { get; set; }

        /// <summary>丸め方法</summary>
        [JsonProperty(PropertyName = "roundMethod")]
        public string RoundMethod { get; set; }
    }
}
