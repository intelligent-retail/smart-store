using Newtonsoft.Json;

namespace ClientService.BoxAdminBff.Models
{
    public abstract class QueryResponse
    {
        public class QueryByTerminalItem
        {
            [JsonProperty("itemCode")]
            public string ItemCode { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }

        public class QueryByStoreItem
        {
            [JsonProperty("terminalCode")]
            public string TerminalCode { get; set; }

            [JsonProperty("itemCode")]
            public string ItemCode { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }
        }
    }

    public class QueryResponse<T> : QueryResponse
    {
        [JsonProperty("items")]
        public T[] Items { get; set; }
    }
}