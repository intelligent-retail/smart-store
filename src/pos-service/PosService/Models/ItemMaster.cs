using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.Models
{
    /// <summary>
    /// 商品マスター
    /// </summary>
    public class ItemMaster
    {
        /// <summary>企業コード</summary>
        [JsonProperty(PropertyName = "companyCode")]
        public string CompanyCode { get; set; }

        /// <summary>店舗コード</summary>
        [JsonProperty(PropertyName = "storeCode")]
        public string StoreCode { get; set; }

        /// <summary>商品コード</summary>
        [JsonProperty(PropertyName = "itemCode")]
        public string ItemCode { get; set; }

        /// <summary>説明</summary>
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        /// <summary>短い説明</summary>
        [JsonProperty(PropertyName = "descriptionShort")]
        public string DescriptionShort { get; set; }

        /// <summary>長い説明</summary>
        [JsonProperty(PropertyName = "descriptionLong")]
        public string DescriptionLong { get; set; }

        /// <summary>メーカーコード</summary>
        [JsonProperty(PropertyName = "manufacturerCode")]
        public string ManufacturerCode { get; set; }

        /// <summary>単価</summary>
        [JsonProperty(PropertyName = "unitPrice")]
        public decimal UnitPrice { get; set; }

        /// <summary>原価</summary>
        [JsonProperty(PropertyName = "unitCost")]
        public decimal UnitCost { get; set; }

        /// <summary>登録日</summary>
        [JsonProperty(PropertyName = "entryDate")]
        public DateTime EntryDate { get; set; }

        /// <summary>最終更新日時</summary>
        [JsonProperty(PropertyName = "lastUpdateDate")]
        public DateTime LastUpdateDate { get; set; }

        /// <summary>商品詳細</summary>
        [JsonProperty(PropertyName = "itemDetails")]
        public List<string> ItemDetails { get; set; }

        /// <summary>商品画像の URL</summary>
        [JsonProperty(PropertyName = "imageUrls")]
        public List<string> ImageUrls { get; set; }

        /// <summary>親階層コード</summary>
        [JsonProperty(PropertyName = "parentCode")]
        public string ParentCode { get; set; }

        /// <summary>親階層レベル</summary>
        [JsonProperty(PropertyName = "parentLevel")]
        public int ParentLevel { get; set; }

        /// <summary>部門コード</summary>
        [JsonProperty(PropertyName = "departmentCode")]
        public string DepartmentCode { get; set; }

        /// <summary>税コード</summary>
        [JsonProperty(PropertyName = "taxCode")]
        public string TaxCode { get; set; }
    }
}
