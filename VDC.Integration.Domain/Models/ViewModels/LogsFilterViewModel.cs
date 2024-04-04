namespace VDC.Integration.Domain.Models.ViewModels
{
    public class LogsFilterViewModel
    {
        public long TenantId { get; set; }
        public string Filter { get; set; }
        public string Method { get; set; }
        public string Type { get; set; }
        public string Webjob { get; set; }
        public int Page { get; set; }
        public string LogId { get; set; }
    }
}
