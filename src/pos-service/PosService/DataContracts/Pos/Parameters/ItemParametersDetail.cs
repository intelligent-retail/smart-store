using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Parameters
{
    /// <summary>
    /// 商品パラメーター詳細
    /// </summary>
    public class ItemParametersDetail
    {
        /// <summary>商品コード</summary>
        [JsonProperty(PropertyName = "itemCode", Required = Required.Always)]
        public string ItemCode { get; set; }

        /// <summary>数量</summary>
        [JsonProperty(PropertyName = "quantity", Required = Required.Always)]
        public int Quantity { get; set; }
    }
}
