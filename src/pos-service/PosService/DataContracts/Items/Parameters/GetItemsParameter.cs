using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Items.Parameters
{
    /// <summary>
    /// 商品情報取得パラメーター
    /// </summary>
    public class GetItemsParameter
    {
        /// <summary>商品コード</summary>
        [JsonProperty(PropertyName = "itemCodes", Required = Required.Always)]
        public List<string> ItemCodes { get; set; }
    }
}
