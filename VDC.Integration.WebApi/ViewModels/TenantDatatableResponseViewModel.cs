using System.Collections.Generic;

namespace VDC.Integration.WebApi.ViewModels
{
    public class TenantDatatableResponseViewModel
    {
        public int Draw { get; set; }
        public int RecordsTotal { get; set; }
        public int RecordsFiltered { get; set; }
        public List<TenantViewModel> Data { get; set; }
        public string Error { get; set; }
    }
}
