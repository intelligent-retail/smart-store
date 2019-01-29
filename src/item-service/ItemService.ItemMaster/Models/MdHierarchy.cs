using System;

using Newtonsoft.Json;

namespace ItemService.ItemMaster.Models
{
    public class MdHierarchy
    {
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("descriptionShort")]
        public string DescriptionShort { get; set; }
        
        [JsonProperty("entryDate")]
        public DateTime EntryDate { get; set; }
        
        [JsonProperty("lastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }
        
        [JsonProperty("mdHierarchyCode")]
        public string MdHierarchyCode { get; set; }
        
        [JsonProperty("mdHierarchyLevel")]
        public int MdHierarchyLevel { get; set; }
        
        [JsonProperty("parentCode")]
        public string ParentCode { get; set; }
        
        [JsonProperty("taxCode")]
        public string TaxCode { get; set; }
    }
}