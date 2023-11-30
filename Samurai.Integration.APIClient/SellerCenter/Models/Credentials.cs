namespace Samurai.Integration.APIClient.SellerCenter.Models
{
    public class Credentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// Optional
        /// </summary>
        public string TenantId { get; set; }
    }
}
