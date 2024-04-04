using System;

namespace VDC.Integration.Domain.Entities.Database
{
    public class MillenniumNewStockProcess
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }
        public DateTime ProcessDate { get; set; }
        public long? InitialTransId { get; set; }
        public long? FinalTransId { get; set; }
        public DateTime? InitialUpdateDate { get; set; }
        public DateTime? FinalUpdateDate { get; set; }
        public int? Top { get; set; }
        public int TotalCount { get; set; }
        public string MillenniumResult { get; set; }
        public string Exception { get; set; }
        public string FirstId { get; set; }
        public string LastId { get; set; }
    }
}
