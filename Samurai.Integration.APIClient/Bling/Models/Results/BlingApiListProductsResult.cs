using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.APIClient.Bling.Models.Results
{
    public class BlingApiListProductsResult
    {
        public Retorno retorno { get; set; }

        public class Retorno
        {
            public List<ProdutoWrapper> produtos { get; set; } = new List<ProdutoWrapper>();
            public List<ErroWrapper> erros { get; set; }
        }

        public class ProdutoWrapper
        {
            public Produto produto { get; set; }
        }

        public class Produto
        {
            public string id { get; set; }
            public string codigo { get; set; }
            public string descricao { get; set; }
            public string tipo { get; set; }
            public string situacao { get; set; }
            public string unidade { get; set; }
            public string preco { get; set; }
            public object precoCusto { get; set; }
            public string descricaoCurta { get; set; }
            public string descricaoComplementar { get; set; }
            public DateTime dataInclusao { get; set; }
            public DateTime dataAlteracao { get; set; }
            public object imageThumbnail { get; set; }
            public string urlVideo { get; set; }
            public string nomeFornecedor { get; set; }
            public string codigoFabricante { get; set; }
            public string marca { get; set; }
            public string class_fiscal { get; set; }
            public string cest { get; set; }
            public string origem { get; set; }
            public string idGrupoProduto { get; set; }
            public string linkExterno { get; set; }
            public string observacoes { get; set; }
            public object grupoProduto { get; set; }
            public object garantia { get; set; }
            public object descricaoFornecedor { get; set; }
            public string idFabricante { get; set; }
            public Categoria categoria { get; set; }
            public string pesoLiq { get; set; }
            public string pesoBruto { get; set; }
            public string estoqueMinimo { get; set; }
            public string estoqueMaximo { get; set; }
            public string gtin { get; set; }
            public string gtinEmbalagem { get; set; }
            public string larguraProduto { get; set; }
            public string alturaProduto { get; set; }
            public string profundidadeProduto { get; set; }
            public string unidadeMedida { get; set; }
            public int itensPorCaixa { get; set; }
            public int volumes { get; set; }
            public string localizacao { get; set; }
            public string crossdocking { get; set; }
            public string condicao { get; set; }
            public string freteGratis { get; set; }
            public string producao { get; set; }
            public string dataValidade { get; set; }
            public string spedTipoItem { get; set; }
            public List<Imagem> imagem { get; set; }
            public string clonarDadosPai { get; set; }
            public string codigoPai { get; set; }
            public decimal estoqueAtual { get; set; }
            public List<DepositoWrapper> depositos { get; set; } = new List<DepositoWrapper>();
            public List<VariacaoWrapper> variacoes { get; set; }
            public string OriginalValue { get; set; }
        }

        public class Imagem
        {
            public string link { get; set; }
        }

        public class Categoria
        {
            public string id { get; set; }
            public string descricao { get; set; }
        }

        public class DepositoWrapper
        {
            public Deposito deposito { get; set; }
        }

        public class Deposito
        {
            public string id { get; set; }
            public string nome { get; set; }
            public decimal saldo { get; set; }
            public string desconsiderar { get; set; }
            public decimal saldoVirtual { get; set; }
        }

        public class VariacaoWrapper
        {
            public Variacao variacao { get; set; }
        }

        public class Variacao
        {
            public string nome { get; set; }
            public string codigo { get; set; }
        }
    }
}
