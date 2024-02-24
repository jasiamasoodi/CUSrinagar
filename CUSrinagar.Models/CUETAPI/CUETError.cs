using System.Net;
using Newtonsoft.Json;

namespace CUSrinagar.Models
{
    public class CUETError
    {
        [JsonProperty("error")]
        public BaseError Error { get; set; }
    }

    public class BaseError
    {
        [JsonProperty("code")]
        public HttpStatusCode Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object[] Data { get; set; }
    }
}
