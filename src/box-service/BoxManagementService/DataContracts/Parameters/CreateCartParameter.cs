using Newtonsoft.Json;

namespace BoxManagementService.DataCotracts.Parameters
{
    /// <summary>
    /// 取引開始要求（カート作成）パラメーター
    /// </summary>
    public class CreateCartParameter
    {
        /// <summary>
        /// BOX識別文字列
        /// </summary>
        [JsonRequired]
        public string BoxId { get; set; }

        /// <summary>
        /// Device識別文字列(スマートフォンのデバイスID)
        /// </summary>
        [JsonRequired]
        public string DeviceId { get; set; }
    }
}
