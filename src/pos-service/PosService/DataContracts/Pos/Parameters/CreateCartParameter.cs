using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Parameters
{
    /// <summary>
    /// カート作成パラメーター
    /// </summary>
    public class CreateCartParameter : ParameterBase
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode", Required = Required.Always)]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode", Required = Required.Always)]
        public string StoreCode { get; set; }

        /// <summary>端末番号</summary>
        [JsonProperty(PropertyName = "terminalNo", Required = Required.Always)]
        public int TerminalNo { get; set; }

        /// <summary>ユーザー ID</summary>
        [JsonProperty(PropertyName = "userId", Required = Required.Always)]
        public string UserId { get; set; }

        /// <summary>ユーザー名</summary>
        [JsonProperty(PropertyName = "userName", Required = Required.Always)]
        public string UserName { get; set; }
    }
}
