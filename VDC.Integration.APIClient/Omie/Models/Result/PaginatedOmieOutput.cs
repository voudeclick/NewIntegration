using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result
{
    public abstract class PaginatedOmieOutput : BaseOmieOutput
    {
        public long pagina { get; set; }
        public long total_de_paginas { get; set; }
        public long registros { get; set; }
        public long total_de_registros { get; set; }
    }
}
