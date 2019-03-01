using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 店舗マスター Document
    /// </summary>
    public class StoreMasterDocument : MasterDocumentBase
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>店舗名</summary>
        [JsonProperty(PropertyName = "storeName")]
        public string StoreName { get; set; }
    }
}
