using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 取引ログベースクラス
    /// </summary>
    public abstract class TransactionLogBase : AbstractDocument
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransactionLogBase()
        {
            this.User = new UserInfo();
            this.Sales = new SalesInfo();
            this.Payments = new List<Payment>();
            this.Taxes = new List<Tax>();
        }

        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>店舗名</summary>
        [JsonProperty(PropertyName = "storeName")]
        public string StoreName { get; set; }

        /// <summary>端末番号</summary>
        [JsonProperty(PropertyName = "terminalNo")]
        public int TerminalNo { get; set; }

        /// <summary>取引番号</summary>
        [JsonProperty(PropertyName = "transactionNo")]
        public long TransactionNo { get; set; }

        /// <summary>取引種別</summary>
        [JsonProperty(PropertyName = "transactionType")]
        public int TransactionType { get; set; }

        /// <summary>営業日</summary>
        [JsonProperty(PropertyName = "businessDate")]
        public DateTime BusinessDate { get; set; }

        /// <summary>生成日時</summary>
        [JsonProperty(PropertyName = "generateDateTime")]
        public DateTime GenerateDateTime { get; set; }

        /// <summary>レシート番号</summary>
        [JsonProperty(PropertyName = "receiptNo")]
        public long ReceiptNo { get; set; }

        /// <summary>ユーザー情報</summary>
        [JsonProperty(PropertyName = "user")]
        public UserInfo User { get; set; }

        /// <summary>売上情報</summary>
        [JsonProperty(PropertyName = "sales")]
        public SalesInfo Sales { get; set; }

        /// <summary>支払情報</summary>
        [JsonProperty(PropertyName = "payments")]
        public List<Payment> Payments { get; set; }

        /// <summary>税情報</summary>
        [JsonProperty(PropertyName = "taxes")]
        public List<Tax> Taxes { get; set; }

        /// <summary>
        /// 売上情報
        /// </summary>
        public class SalesInfo
        {
            /// <summary>参照日時</summary>
            [JsonProperty(PropertyName = "referenceDateTime")]
            public DateTime ReferenceDateTime { get; set; }

            /// <summary>合計金額</summary>
            [JsonProperty(PropertyName = "totalAmount")]
            public decimal TotalAmount { get; set; }

            /// <summary>税込み合計金額</summary>
            [JsonProperty(PropertyName = "totalAmountWithTaxes")]
            public decimal TotalAmountWithTaxes { get; set; }

            /// <summary>税額</summary>
            [JsonProperty(PropertyName = "taxesAmount")]
            public decimal TaxesAmount { get; set; }

            /// <summary>合計数量</summary>
            [JsonProperty(PropertyName = "totalQuantity")]
            public int TotalQuantity { get; set; }

            /// <summary>つり銭額</summary>
            [JsonProperty(PropertyName = "changeAmount")]
            public decimal ChangeAmount { get; set; }

            /// <summary>取引中止かどうか</summary>
            [JsonProperty(PropertyName = "isCanceled")]
            public bool IsCanceled { get; set; }
        }

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

            /// <summary>部門コード</summary>
            [JsonProperty(PropertyName = "departmentCode")]
            public string DepartmentCode { get; set; }

            /// <summary>説明</summary>
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }

            /// <summary>短い説明</summary>
            [JsonProperty(PropertyName = "descriptionShort")]
            public string DescriptionShort { get; set; }

            /// <summary>単価</summary>
            [JsonProperty(PropertyName = "unitPrice")]
            public decimal UnitPrice { get; set; }

            /// <summary>数量</summary>
            [JsonProperty(PropertyName = "quantity")]
            public int Quantity { get; set; }

            /// <summary>返品数量</summary>
            [JsonProperty(PropertyName = "returnedQuantity")]
            public int ReturnedQuantity { get; set; }

            /// <summary>金額</summary>
            [JsonProperty(PropertyName = "amount")]
            public decimal Amount { get; set; }

            /// <summary>税コード</summary>
            [JsonProperty(PropertyName = "taxCode")]
            public string TaxCode { get; set; }
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

            /// <summary>預り金額</summary>
            [JsonProperty(PropertyName = "depositAmount")]
            public decimal DepositAmount { get; set; }

            /// <summary>支払金額</summary>
            [JsonProperty(PropertyName = "amount")]
            public decimal Amount { get; set; }

            /// <summary>説明</summary>
            [JsonProperty(PropertyName = "description")]
            public string Description { get; set; }
        }

        /// <summary>
        /// 税情報
        /// </summary>
        public class Tax
        {
            /// <summary>税番号</summary>
            [JsonProperty(PropertyName = "taxNo")]
            public int TaxNo { get; set; }

            /// <summary>税コード</summary>
            [JsonProperty(PropertyName = "taxCode")]
            public string TaxCode { get; set; }

            /// <summary>税額</summary>
            [JsonProperty(PropertyName = "taxAmount")]
            public decimal TaxAmount { get; set; }

            /// <summary>対象金額</summary>
            [JsonProperty(PropertyName = "targetAmount")]
            public decimal TargetAmount { get; set; }

            /// <summary>対象数量</summary>
            [JsonProperty(PropertyName = "targetQuantity")]
            public decimal TargetQuantity { get; set; }
        }
    }
}
