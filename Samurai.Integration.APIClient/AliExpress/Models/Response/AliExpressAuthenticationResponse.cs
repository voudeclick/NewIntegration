using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.AliExpress.Models.Response
{
    public class AliExpressAuthenticationResponse
    {
        [JsonPropertyName("appKey")]
        public string AppKey { get; set; }

        [JsonPropertyName("appSecret")]
        public string AppSecret { get; set; }

        [JsonPropertyName("storeId")]
        public string StoreId { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("tokenIsValid")]
        public bool TokenIsValid { get; set; }
    }
}
