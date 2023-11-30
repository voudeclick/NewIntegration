using System;

namespace Samurai.Integration.Domain.Entities.Database
{
    public class MillenniumOrderStatusUpdate
    {
        public Guid Id { get; set; }
        public long TenantId { get; set; }   
        public string CodPedidov { get; set; }
        public int OrderStatus { get; set; }
        public string Order { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
