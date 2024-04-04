using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Requests
{
    public class MillenniumApiListStockMtoRequest
    {
        public long? TransId { get; set; }
        public string EstrategiaProducao { get; set; }
        public int? Top { get; set; }
        public List<string> Filiais { get; set; }

    }
}
