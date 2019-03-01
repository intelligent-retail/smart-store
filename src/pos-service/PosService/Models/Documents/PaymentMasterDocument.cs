using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 支払マスター Document
    /// </summary>
    public class PaymentMasterDocument : MasterDocumentBase
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>支払コード</summary>
        [JsonProperty(PropertyName = "paymentCode")]
        public string PaymentCode { get; set; }

        /// <summary>説明</summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
