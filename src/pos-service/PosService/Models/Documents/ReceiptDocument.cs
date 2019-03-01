using System;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// レシート Document
    /// </summary>
    public class ReceiptDocument : AbstractDocument
    {
        /// <summary>パーティションキー</summary>
        [JsonProperty(PropertyName = "key")]
        public string Key
        {
            get
            {
                return $"{this.CompanyCode}_{this.StoreCode}_{this.BusinessDate.ToString("yyyyMM")}";
            }
        }

        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

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

        /// <summary>レシートテキスト</summary>
        [JsonProperty(PropertyName = "receiptText")]
        public string ReceiptText { get; set; }

        /// <summary>ユーザー情報</summary>
        [JsonProperty(PropertyName = "user")]
        public UserInfo User { get; set; }
    }
}
