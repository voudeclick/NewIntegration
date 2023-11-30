
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ProdutoCadastro.Inputs
{
    public class ListarProdutosOmieRequestInput : BaseOmieInput
    {
        public int pagina { get; set; } = 1;
        public int registros_por_pagina { get; set; } = 50;
        public string filtrar_apenas_omiepdv { get; set; } = "N";
        public string apenas_importado_api { get; set; } = "N";
        public string filtrar_apenas_marketplace { get; set; }
        public string filtrar_apenas_familia { get; set; }
        public string exibir_caracteristicas { get; set; }
        public string exibir_obs { get; set; }
        public List<ProdutoCodigo> produtosPorCodigo { get; set; }

        public class ProdutoCodigo
        {
            public long codigo_produto { get; set; }
        }
    }
}
