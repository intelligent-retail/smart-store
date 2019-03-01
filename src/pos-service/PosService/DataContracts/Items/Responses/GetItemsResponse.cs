using System;
using System.Collections.Generic;
using PosService.Models;
using Newtonsoft.Json;

namespace PosService.DataContracts.Items.Responses
{
    /// <summary>
    /// 商品情報取得結果
    /// </summary>
    public class GetItemsResponse
    {
        /// <summary>商品情報一覧</summary>
        [JsonProperty(PropertyName = "items")]
        public List<GetItemsResponseDetail> Items { get; set; }

        /// <summary>
        /// 商品情報
        /// </summary>
        public class GetItemsResponseDetail
        {
            /// <summary>ID</summary>
            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

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

            /// <summary>MD 階層マスター</summary>
            [JsonProperty(PropertyName = "mdHierarchies")]
            public List<MdhierarchyMaster> MdHierarchies { get; set; }

            /// <summary>税マスター</summary>
            [JsonProperty(PropertyName = "tax")]
            public TaxMaster Tax { get; set; }
        }
    }
}
