using System;
using Newtonsoft.Json;

namespace PosService.Models.Documents
{
    public abstract class AbstractDocument
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("createdOn")]
        public DateTime CreatedOn { get; set; }

        [JsonProperty("updatedOn")]
        public DateTime UpdatedOn { get; set; }

        [JsonProperty("_etag")]
        public string Etag { get; set; }
    }
}
