using Samurai.Integration.APIClient.Omie.Models.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result
{
    public abstract class PaginatedOmieOutput : BaseOmieOutput
    {
        public long pagina { get; set; }
        public long total_de_paginas { get; set; }
        public long registros { get; set; }
        public long total_de_registros { get; set; }
    }
}
