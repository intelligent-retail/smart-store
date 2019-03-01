using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxManagementService.DataCotracts.Parameters
{
    /// <summary>
    /// 商品更新のパラメーターです。
    /// </summary>
    public class UpdateItemsParameter
    {
        /// <summary>
        /// BOX 識別文字列を取得または設定します。
        /// </summary>
        [JsonProperty("box_id", Required = Required.Always)]
        public string BoxId { get; set; }

        /// <summary>
        /// 数量に変動があった商品のリストを取得または設定します。
        /// </summary>
        [JsonProperty("items", Required = Required.Always)]
        public IList<Item> Items { get; set; }

        /// <summary>
        /// 商品を表します。
        /// </summary>
        public class Item
        {
            /// <summary>
            /// SKU コードを取得または設定します。
            /// </summary>
            [JsonProperty("skuCode", Required = Required.Always)]
            public string SkuCode { get; set; }

            /// <summary>
            /// 数量の増減値を取得または設定します。
            /// </summary>
            [JsonProperty("quantity", Required = Required.Always)]
            public int Quantity { get; set; }
        }
    }
}