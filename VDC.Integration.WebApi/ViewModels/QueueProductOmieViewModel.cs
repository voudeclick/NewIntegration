namespace VDC.Integration.WebApi.ViewModels
{
    public class QueueProductOmieViewModel
    {
        public long TenantId { get; set; }

        public long? ProductId { get; set; }

        public bool? ListAllProducts { get; set; }
    }
}
