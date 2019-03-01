using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.DataContracts.Pos.Responses
{
    /// <summary>
    /// カート取得結果
    /// </summary>
    public class GetCartResponse : ResponseBase
    {
        /// <summary>店舗情報</summary>
        [JsonProperty(PropertyName = "store")]
        public StoreInfo Store { get; set; }

        /// <summary>ユーザー情報</summary>
        [JsonProperty(PropertyName = "user")]
        public UserInfo User { get; set; }

        /// <summary>カート情報</summary>
        [JsonProperty(PropertyName = "cart")]
        public CartInfo Cart { get; set; }

        /// <summary>
        /// 店舗情報
        /// </summary>
        public class StoreInfo
        {
            /// <summary>店舗コード</summary>
            [JsonProperty(PropertyName = "storeCode")]
            public string StoreCode { get; set; }

            /// <summary>店舗名</summary>
            [JsonProperty(PropertyName = "storeName")]
            public string StoreName { get; set; }

            /// <summary>端末番号</summary>
            [JsonProperty(PropertyName = "terminalNo")]
            public int TerminalNo { get; set; }
        }

        /// <summary>
        /// ユーザー情報
        /// </summary>
        public class UserInfo
        {
            /// <summary>ユーザー ID</summary>
            [JsonProperty(PropertyName = "userId")]
            public string UserId { get; set; }

            /// <summary>ユーザー名</summary>
            [JsonProperty(PropertyName = "userName")]
            public string UserName { get; set; }
        }

        /// <summary>
        /// カート情報
        /// </summary>
        public class CartInfo
        {
            /// <summary>カート ID</summary>
            [JsonProperty(PropertyName = "cartId")]
            public string CartId { get; set; }

            /// <summary>合計金額</summary>
            [JsonProperty(PropertyName = "totalAmount")]
            public decimal TotalAmount { get; set; }

            /// <summary>小計金額</summary>
            [JsonProperty(PropertyName = "subtotalAmount")]
            public decimal SubtotalAmount { get; set; }

            /// <summary>合計数量</summary>
            [JsonProperty(PropertyName = "totalQuantity")]
            public int TotalQuantity { get; set; }

            /// <summary>レシート番号</summary>
            [JsonProperty(PropertyName = "receiptNo")]
            public long ReceiptNo { get; set; }

            /// <summary>レシートテキスト</summary>
            [JsonProperty(PropertyName = "receiptText")]
            public string ReceiptText { get; set; }

            /// <summary>預り金額</summary>
            [JsonProperty(PropertyName = "depositAmount")]
            public decimal DepositAmount { get; set; }

            /// <summary>つり銭額</summary>
            [JsonProperty(PropertyName = "changeAmount")]
            public decimal ChangeAmount { get; set; }

            /// <summary>残額</summary>
            [JsonProperty(PropertyName = "balance")]
            public decimal Balance { get; set; }

            /// <summary>取引番号</summary>
            [JsonProperty(PropertyName = "transactionNo")]
            public long TransactionNo { get; set; }

            /// <summary>カート状態</summary>
            [JsonProperty(PropertyName = "cartStatus")]
            public string CartStatus { get; set; }

            /// <summary>商品明細</summary>
            [JsonProperty(PropertyName = "lineItems")]
            public List<LineItem> LineItems { get; set; }

            /// <summary>支払情報</summary>
            [JsonProperty(PropertyName = "payments")]
            public List<Payment> Payments { get; set; }

            /// <summary>税情報</summary>
            [JsonProperty(PropertyName = "taxes")]
            public List<Tax> Taxes { get; set; }

            /// <summary>
            /// 商品明細
            /// </summary>
            public class LineItem
            {
                /// <summary>明細番号</summary>
                [JsonProperty(PropertyName = "lineNo")]
                public int LineNo { get; set; }

                /// <summary>商品コード</summary>
                [JsonProperty(PropertyName = "itemCode")]
                public string ItemCode { get; set; }

                /// <summary>商品名</summary>
                [JsonProperty(PropertyName = "itemName")]
                public string ItemName { get; set; }

                /// <summary>単価</summary>
                [JsonProperty(PropertyName = "unitPrice")]
                public decimal UnitPrice { get; set; }

                /// <summary>数量</summary>
                [JsonProperty(PropertyName = "quantity")]
                public int Quantity { get; set; }

                /// <summary>金額</summary>
                [JsonProperty(PropertyName = "amount")]
                public decimal Amount { get; set; }

                /// <summary>商品画像の URL</summary>
                [JsonProperty(PropertyName = "imageUrls")]
                public List<string> ImageUrls { get; set; }
            }

            /// <summary>
            /// 支払情報
            /// </summary>
            public class Payment
            {
                /// <summary>支払番号</summary>
                [JsonProperty(PropertyName = "paymentNo")]
                public int PaymentNo { get; set; }

                /// <summary>支払コード</summary>
                [JsonProperty(PropertyName = "paymentCode")]
                public string PaymentCode { get; set; }

                /// <summary>支払名称</summary>
                [JsonProperty(PropertyName = "paymentName")]
                public string PaymentName { get; set; }

                /// <summary>支払金額</summary>
                [JsonProperty(PropertyName = "paymentAmount")]
                public decimal PaymentAmount { get; set; }
            }

            /// <summary>
            /// 税情報
            /// </summary>
            public class Tax
            {
                /// <summary>税番号</summary>
                [JsonProperty(PropertyName = "taxNo")]
                public int TaxNo { get; set; }

                /// <summary>税名称</summary>
                [JsonProperty(PropertyName = "taxName")]
                public string TaxName { get; set; }

                /// <summary>税額</summary>
                [JsonProperty(PropertyName = "taxAmount")]
                public decimal TaxAmount { get; set; }
            }
        }
    }
}
