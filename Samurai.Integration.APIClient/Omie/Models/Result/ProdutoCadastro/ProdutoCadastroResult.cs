using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Omie.Models.Result.ProdutoCadastro
{
    public class ProdutoCadastroResult
    {
        public string codigo { get; set; }
        public long codigo_produto { get; set; }
        public string codigo_produto_integracao { get; set; }
        public long codigo_familia { get; set; }
        public string codInt_familia { get; set; }
        public string descricao_familia { get; set; }
        public string descricao { get; set; }
        public string descr_detalhada { get; set; }
        public string marca { get; set; }
        public decimal altura { get; set; }
        public decimal largura { get; set; }
        public decimal profundidade { get; set; }
        public decimal peso_bruto { get; set; } //Kg
        public decimal peso_liq { get; set; }
        public decimal valor_unitario { get; set; }
        public string ean { get; set; }
        public string ncm { get; set; }
        public string obs_internas { get; set; }
        public string inativo { get; set; }
        public Info info { get; set; }
        public List<Caracteristica> caracteristicas { get; set; }
        public List<Imagem> imagens { get; set; }

        public class Info
        {
            public string dAlt { get; set; }
            public string dInc { get; set; }
            public string hAlt { get; set; }
            public string hInc { get; set; }
            public string uAlt { get; set; }
            public string uInc { get; set; }
        }
        public class Caracteristica
        {
            public string cCodIntCaract { get; set; }
            public string cConteudo { get; set; }
            public string cExibirItemNF { get; set; }
            public string cExibirItemPedido { get; set; }
            public string cNomeCaract { get; set; }
            public long nCodCaract { get; set; }
        }

        public class Imagem
        {
            public string url_imagem { get; set; }
        }
    }
}