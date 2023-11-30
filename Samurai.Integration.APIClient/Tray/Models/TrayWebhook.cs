namespace Samurai.Integration.APIClient.Tray.Models
{
    public class TrayWebhook
    {
        /// <summary>
        /// Ação realizada
        /// </summary>
        public string act { get; set; }
        public string app_code { get; set; }
        
        /// <summary>
        /// Código do escopo da notificação.
        /// </summary>
        public long scope_id { get; set; }

        /// <summary>
        /// Nome do escopo notificado.
        /// </summary>
        public string scope_name { get; set; }

        /// <summary>
        /// Código do vendedor
        /// </summary>
        public long seller_id { get; set; }
        public string url_notification { get; set; }

        public string Topic => ($"{scope_name}_{act}").ToLower().Trim();
    }
}
