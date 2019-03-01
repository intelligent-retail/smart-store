using System.Collections.Generic;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    /// <summary>
    /// カート Document
    /// </summary>
    public class CartDocument : TransactionLogBase
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CartDocument()
        {
            this.LineItems = new List<CartLineItem>();

            this.Masters = new ReferenceMasters
            {
                Items = new List<ItemMaster>(),
                MdHierarchies = new List<MdhierarchyMaster>(),
                Taxes = new List<TaxMaster>()
            };
        }

        /// <summary>カート ID</summary>
        [JsonProperty(PropertyName = "cartId")]
        public string CartId { get; set; }

        /// <summary>カート状態</summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>レシートテキスト</summary>
        [JsonProperty(PropertyName = "receiptText")]
        public string ReceiptText { get; set; }

        /// <summary>カート商品明細</summary>
        [JsonProperty(PropertyName = "items")]
        public List<CartLineItem> LineItems { get; set; }

        /// <summary>参照マスター情報</summary>
        [JsonProperty(PropertyName = "masters")]
        public ReferenceMasters Masters { get; set; }

        /// <summary>
        /// カート商品明細
        /// </summary>
        public class CartLineItem : LineItem
        {
            /// <summary>商品詳細</summary>
            [JsonProperty(PropertyName = "itemDetails")]
            public List<string> ItemDetails { get; set; }

            /// <summary>商品画像の URL</summary>
            [JsonProperty(PropertyName = "imageUrls")]
            public List<string> ImageUrls { get; set; }
        }

        /// <summary>
        /// 参照マスター情報
        /// </summary>
        public class ReferenceMasters
        {
            /// <summary>商品マスター</summary>
            [JsonProperty(PropertyName = "items")]
            public List<ItemMaster> Items { get; set; }

            /// <summary>MD 階層マスター</summary>
            [JsonProperty(PropertyName = "mdHierarchies")]
            public List<MdhierarchyMaster> MdHierarchies { get; set; }

            /// <summary>税マスター</summary>
            [JsonProperty(PropertyName = "taxes")]
            public List<TaxMaster> Taxes { get; set; }
        }
    }
}
