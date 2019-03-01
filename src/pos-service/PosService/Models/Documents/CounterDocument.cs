using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// ビジネスカウンター Document
    /// </summary>
    public class CounterDocument : AbstractDocument
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CounterDocument()
        {
            this.CountDic = new Dictionary<string, long>();
        }

        /// <summary>端末キー</summary>
        [JsonProperty(PropertyName = "terminalKey")]
        public string TerminalKey { get; set; }

        /// <summary>
        /// カウンター辞書
        /// Key:種別
        /// Val:数
        /// </summary>
        [JsonProperty(PropertyName = "countDic")]
        public Dictionary<string, long> CountDic { get; set; }
    }
}
