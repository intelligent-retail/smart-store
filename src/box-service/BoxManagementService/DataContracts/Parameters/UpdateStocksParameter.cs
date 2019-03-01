using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BoxManagementService.DataCotracts.Parameters
{
    /// <summary>
    /// 在庫更新のパラメーターです。
    /// </summary>
    public class UpdateStocksParameter
    {
        /// <summary>
        /// BOX 識別文字列を取得または設定します。
        /// </summary>
        [JsonProperty("box_id", Required = Required.Always)]
        public string BoxId { get; set; }

        /// <summary>
        /// BOX で付与されたタイムスタンプを取得または設定します。
        /// </summary>
        [JsonProperty("DataTimestamp", Required = Required.Always)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// BOX 内のすべての商品のリストを取得または設定します。
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
            /// 数量を取得または設定します。
            /// </summary>
            [JsonProperty("quantity", Required = Required.Always)]
            public int Quantity { get; set; }
        }
    }
}