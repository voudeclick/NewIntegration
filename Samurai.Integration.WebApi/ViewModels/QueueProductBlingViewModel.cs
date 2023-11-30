namespace Samurai.Integration.WebApi.ViewModels
{
    public class QueueProductBlingViewModel
    {
        public long TenantId { get; set; }

        public string? ProductId { get; set; }

        public bool? ListAllProducts { get; set; }
    }
}
