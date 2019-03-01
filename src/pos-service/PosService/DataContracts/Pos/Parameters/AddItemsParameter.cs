using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Parameters
{
    /// <summary>
    /// 商品追加パラメーター
    /// </summary>
    public class AddItemsParameter : ParameterBase
    {
        /// <summary>追加する商品情報</summary>
        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public List<ItemParametersDetail> Items { get; set; }
    }
}
