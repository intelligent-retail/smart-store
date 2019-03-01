using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 企業マスター Document
    /// </summary>
    public class CompanyMasterDocument : MasterDocumentBase
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>企業名</summary>
        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }
    }
}
