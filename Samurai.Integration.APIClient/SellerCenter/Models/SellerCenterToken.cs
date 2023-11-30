using Newtonsoft.Json;

namespace Samurai.Integration.APIClient.SellerCenter.Models
{
    public class SellerCenterToken
    {     
        
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public double ExpiresIn { get; set; }
    }
}
