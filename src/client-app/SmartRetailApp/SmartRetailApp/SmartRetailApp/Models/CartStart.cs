using Newtonsoft.Json;

namespace SmartRetailApp.Models
{
    /// <summary>
    /// 取引を開始するクラス
    /// </summary>
    public class CartStart
    {
        /// <summary>
        /// ボックスのID
        /// </summary>
        [JsonProperty(PropertyName = "boxId")]
        public string BoxId { get; set; }

        /// <summary>
        /// デバイスのID
        /// </summary>
        [JsonProperty(PropertyName = "deviceId")]
        public string DeviceId { get; set; }
    }
}
