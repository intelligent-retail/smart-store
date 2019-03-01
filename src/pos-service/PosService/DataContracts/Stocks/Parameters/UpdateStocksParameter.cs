using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Items.Parameters
{
    /// <summary>
    /// 在庫更新パラメーター
    /// </summary>
    public class UpdateStocksParameter
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UpdateStocksParameter()
        {
            this.Items = new List<StockItem>();
        }

        /// <summary>取引 ID</summary>
        [JsonProperty(PropertyName = "transactionId", Required = Required.Always)]
        public long TransactionId { get; set; }

        /// <summary>取引日時</summary>
        [JsonProperty(PropertyName = "transactionDate", Required = Required.Always)]
        public DateTime TransactionDate { get; set; }

        /// <summary>取引種別</summary>
        [JsonProperty(PropertyName = "transactionType", Required = Required.Always)]
        public string TransactionType { get; set; }

        /// <summary>ロケーションコード</summary>
        [JsonProperty(PropertyName = "locationCode", Required = Required.Always)]
        public string LocationCode { get; set; }

        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode", Required = Required.Always)]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode", Required = Required.Always)]
        public string StoreCode { get; set; }

        /// <summary>端末コード</summary>
        [JsonProperty(PropertyName = "terminalCode", Required = Required.Always)]
        public int TerminalCode { get; set; }

        /// <summary>在庫商品詳細</summary>
        [JsonProperty(PropertyName = "items", Required = Required.Always)]
        public List<StockItem> Items { get; set; }

        /// <summary>
        /// 在庫商品詳細
        /// </summary>
        public class StockItem
        {
            /// <summary>明細番号</summary>
            [JsonProperty(PropertyName = "lineNo", Required = Required.Always)]
            public int LineNo { get; set; }

            /// <summary>商品コード</summary>
            [JsonProperty(PropertyName = "itemCode", Required = Required.Always)]
            public string ItemCode { get; set; }

            /// <summary>数量</summary>
            [JsonProperty(PropertyName = "quantity", Required = Required.Always)]
            public int Quantity { get; set; }
        }
    }
}
