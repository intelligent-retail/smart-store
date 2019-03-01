using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Parameters
{
    /// <summary>
    /// 小計パラメーター
    /// </summary>
    public class SubtotalParameter : ParameterBase
    {
        /// <summary>洗替する商品情報</summary>
        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public List<ItemParametersDetail> Items { get; set; }
    }
}
