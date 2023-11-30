using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Tray.Models.Response
{
    public class TrayAuthenticationResponse
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("date_expiration_access_token")]
        public DateTime DateExpirationAccessToken { get; set; }

        [JsonProperty("date_expiration_refresh_token")]
        public DateTime DateExpirationRefreshToken { get; set; }

        [JsonProperty("date_activated")]
        public DateTime DateActivated { get; set; }

        [JsonProperty("api_host")]
        public string ApiHost { get; set; }

        [JsonProperty("store_id")]
        public long StoreId { get; set; }
    }
}
