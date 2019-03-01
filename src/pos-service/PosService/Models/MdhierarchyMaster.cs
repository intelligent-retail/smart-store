using System;
using Newtonsoft.Json;

namespace PosService.Models
{
    /// <summary>
    /// MD 階層マスター
    /// </summary>
    public class MdhierarchyMaster
    {
        /// <summary>MD 階層コード</summary>
        [JsonProperty(PropertyName = "mdHierarchyCode")]
        public string MdHierarchyCode { get; set; }

        /// <summary>MD 階層レベル</summary>
        [JsonProperty(PropertyName = "mdHierarchyLevel")]
        public int MdHierarchyLevel { get; set; }

        /// <summary>親階層コード</summary>
        [JsonProperty(PropertyName = "parentCode")]
        public string ParentCode { get; set; }

        /// <summary>税コード</summary>
        [JsonProperty(PropertyName = "taxCode")]
        public string TaxCode { get; set; }

        /// <summary>説明</summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>短い説明</summary>
        [JsonProperty(PropertyName = "descriptionShort")]
        public string DescriptionShort { get; set; }

        /// <summary>登録日</summary>
        [JsonProperty(PropertyName = "entryDate")]
        public DateTime EntryDate { get; set; }

        /// <summary>最終更新日時</summary>
        [JsonProperty(PropertyName = "lastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }
    }
}
