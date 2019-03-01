using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 端末マスター Document
    /// </summary>
    public class TerminalMasterDocument : MasterDocumentBase
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>端末番号</summary>
        [JsonProperty(PropertyName = "terminalNo")]
        public int TerminalNo { get; set; }

        /// <summary>端末名</summary>
        [JsonProperty(PropertyName = "terminalName")]
        public string TerminalName { get; set; }

        /// <summary>端末種別 1</summary>
        [JsonProperty(PropertyName = "terminalType1")]
        public string TerminalType1 { get; set; }

        /// <summary>端末種別 2</summary>
        [JsonProperty(PropertyName = "terminalType2")]
        public string TerminalType2 { get; set; }
    }
}
