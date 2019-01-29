using System;

using Newtonsoft.Json;

namespace StockService.Documents
{
    public class StockDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("transactionId")]
        public long TransactionId { get; set; }

        [JsonProperty("transactionDate")]
        public DateTime TransactionDate { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("locationCode")]
        public string LocationCode { get; set; }

        [JsonProperty("companyCode")]
        public string CompanyCode { get; set; }

        [JsonProperty("storeCode")]
        public string StoreCode { get; set; }

        [JsonProperty("terminalCode")]
        public string TerminalCode { get; set; }

        [JsonProperty("items")]
        public StockItemDocument[] Items { get; set; }

        [JsonProperty("_activityId")]
        public string ActivityId { get; set; }
    }

    public class StockItemDocument
    {
        [JsonProperty("lineNo")]
        public int LineNo { get; set; }

        [JsonProperty("itemCode")]
        public string ItemCode { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }
    }
}