using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.LocalEstoque.Inputs
{
    public class ListarLocaisEstoqueOmieRequestInput : BaseOmieInput
    {
        public int nPagina { get; set; } = 1;
        public int nRegPorPagina { get; set; } = 50;
    }
}
