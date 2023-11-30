using Samurai.Integration.Domain.Enums;
using System.Collections.Generic;

namespace Samurai.Integration.WebApi.ViewModels
{
    public class TenantDatatableRequestViewModel
    {
        public bool? Status { get; set; }
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public List<Dictionary<string,string>> Columns { get; set; }
        public Dictionary<string, string> Search { get; set; }
        public List<Dictionary<string, string>> Order { get; set; }

        public string ERP { get; set; }
        public string Shop { get; set; }
    }

    public class TenantColumnRequestViewModel
    {
        public string Data { get; set; }
        public string Name { get; set; }
        public bool Searchable { get; set; }
        public bool Orderable { get; set; }
        public TenantSearchRequestViewModel Search { get; set; }

    }

    public class TenantSearchRequestViewModel
    {
        public string Value { get; set; }
        public bool Regex { get; set; }
    }

    public class TenantOrderRequestViewModel
    {
        public int Column { get; set; }
        public string Dir { get; set; }
    }

}
