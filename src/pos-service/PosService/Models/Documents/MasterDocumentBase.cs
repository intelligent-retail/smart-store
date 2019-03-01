using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// マスター Document ベースクラス
    /// </summary>
    public class MasterDocumentBase : AbstractDocument
    {
        /// <summary>マスター名</summary>
        [JsonProperty(PropertyName = "mastername")]
        public string Mastername { get; set; }
    }
}
