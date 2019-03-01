using Newtonsoft.Json;

namespace BoxManagementService.Models.Repositories
{
    /// <summary>
    /// 端末情報
    /// </summary>
    public class Terminal
    {
        /// <summary>
        /// ID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// BOXの識別文字列
        /// </summary>
        [JsonProperty(PropertyName = "boxId")]
        public string BoxId { get; set; }

        /// <summary>
        /// 法人コード
        /// POS側での端末特定用
        /// </summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>
        /// 店舗コード
        /// POS側での端末特定用
        /// </summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>
        /// 端末番号
        /// POS側での端末特定用
        /// </summary>
        [JsonProperty(PropertyName = "terminalNo")]
        public int TerminalNo { get; set; }

        /// <summary>
        /// 端末名
        /// </summary>
        [JsonProperty(PropertyName = "terminalName")]
        public string TerminalName { get; set; }

        /// <summary>
        /// BOXタイプ
        /// </summary>
        [JsonProperty(PropertyName = "boxType")]
        public string BoxType { get; set; }

        /// <summary>
        /// BOXのサイズ
        /// </summary>
        [JsonProperty(PropertyName = "boxSize")]
        public string BoxSize { get; set; }

        /// <summary>
        /// BOXにC2Dでメッセージを送るときのIoTHubデバイスID
        /// </summary>
        [JsonProperty(PropertyName = "boxDeviceId")]
        public string BoxDeviceId { get; set; }
    }
}
