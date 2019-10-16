using System.Collections.Generic;
using Newtonsoft.Json;

namespace CluedIn.ExternalSearch.Providers.Openfigi.Models
{

    public class Response
    {

        [JsonProperty("figi")]
        public string Figi { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        [JsonProperty("exchCode")]
        public string ExchCode { get; set; }

        [JsonProperty("compositeFIGI")]
        public string CompositeFIGI { get; set; }

        [JsonProperty("uniqueID")]
        public string UniqueID { get; set; }

        [JsonProperty("securityType")]
        public string SecurityType { get; set; }

        [JsonProperty("marketSector")]
        public string MarketSector { get; set; }

        [JsonProperty("shareClassFIGI")]
        public string ShareClassFIGI { get; set; }

        [JsonProperty("uniqueIDFutOpt")]
        public object UniqueIDFutOpt { get; set; }

        [JsonProperty("securityType2")]
        public string SecurityType2 { get; set; }

        [JsonProperty("securityDescription")]
        public string SecurityDescription { get; set; }
    }

    public class MappingResponse
    {

        [JsonProperty("data")]
        public Response Response { get; set; }
    }

}
