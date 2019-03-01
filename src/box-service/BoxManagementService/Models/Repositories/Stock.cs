using Newtonsoft.Json;
using System;

namespace BoxManagementService.Models.Repositories
{
    public class Stock
    {
        /// <summary>
        /// CosmosDBで必要なID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// BOX識別文字列
        /// </summary>
        [JsonProperty(PropertyName = "boxId")]
        public string BoxId { get; set; }

        /// <summary>
        /// イベント区分（0:Open、1:Close）
        /// </summary>
        [JsonProperty(PropertyName = "eventType")]
        public string EventType { get; set; }

        /// <summary>
        /// SKUコード
        /// </summary>
        [JsonProperty(PropertyName = "skuCode")]
        public string SkuCode { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        [JsonProperty(PropertyName = "updatedAt")]
        public DateTime UpdatedAt { get; set; }
    }
}
