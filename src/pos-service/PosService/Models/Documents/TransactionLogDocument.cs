using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// 取引ログ Document
    /// </summary>
    public class TransactionLogDocument : TransactionLogBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TransactionLogDocument()
        {
            this.LineItems = new List<LineItem>();
        }

        /// <summary>パーティションキー</summary>
        [JsonProperty(PropertyName = "key")]
        public string Key
        {
            get
            {
                return $"{this.CompanyCode}_{this.StoreCode}_{this.BusinessDate.ToString("yyyyMM")}";
            }
        }

        /// <summary>バージョン情報</summary>
        [JsonProperty(PropertyName = "version")]
        public VersionInfo Version => new VersionInfo() { CurrentVersion = "v1.0" };

        /// <summary>商品明細</summary>
        [JsonProperty(PropertyName = "items")]
        public List<LineItem> LineItems { get; set; }

        /// <summary>
        /// バージョン情報
        /// </summary>
        public class VersionInfo
        {
            /// <summary>現在のバージョン</summary>
            [JsonProperty(PropertyName = "currentVersion")]
            public string CurrentVersion { get; set; }
        }
    }
}
