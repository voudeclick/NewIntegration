using Samurai.Integration.Domain.Models.Millennium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiListProductsResult
    {
        public List<dynamic> value { get; set; }

        public List<Value> GetValues()
        {
            return value.Select(x =>
            {
                var stringValue = System.Text.Json.JsonSerializer.Serialize(x);
                Value value = System.Text.Json.JsonSerializer.Deserialize<Value>(stringValue);
                value.OriginalValue = stringValue;
                return value;
            }).ToList();
        }

        public class Value
        {
            public long produto { get; set; }
            public string cod_produto { get; set; }
            public string descricao1 { get; set; }
            public string tipo_prod { get; set; }
            public string descricao_traduzida { get; set; }
            public string descricao_literal { get; set; }
            public string descricao_etiq { get; set; }
            public string descricao_sf { get; set; }
            public List<Sku> sku { get; set; }
            public int? colecao { get; set; }
            public int? marca { get; set; }
            public string cod_marca { get; set; }
            public string desc_marca { get; set; }
            public string obs { get; set; }
            public List<Classificaco> classificacoes { get; set; }
            public string codigo_youtube { get; set; }
            public string ativo { get; set; }
            public string permite_pedido_sem_estoque { get; set; }
            public int? lead_time_entrega { get; set; }
            public string referencia { get; set; }
            public string data_alteracao { get; set; }
            public string nome_produto_site { get; set; }
            public string palavra_chave { get; set; }
            public string descricao_produto_site { get; set; }
            public string descricao_produto_anuncio { get; set; }
            public bool excluido { get; set; }
            public string expressao_imagem { get; set; }
            public bool? exibir_prod_sem_est { get; set; }
            public bool kit { get; set; }
            public string cod_isbn { get; set; }
            public decimal? largura { get; set; }
            public decimal? altura { get; set; }
            public decimal? comprimento { get; set; }
            public string data_cadastro { get; set; }
            public string data_alteracao_iso { get; set; }
            public long trans_id { get; set; }
            public string visibilidade { get; set; }
            public int vitrine { get; set; }
            public string web { get; set; }
            public string nome_complemento { get; set; }
            public decimal? peso { get; set; }
            public string cod_produto_orig { get; set; }
            public bool? produto_gratis { get; set; }
            public decimal? largura_real { get; set; }
            public decimal? altura_real { get; set; }
            public decimal? comprimento_real { get; set; }
            public decimal? peso_real { get; set; }
            public string OriginalValue { get; set; }

            public List<MillenniumApiListProductsResult.Sku> ActivesSkus => sku.Where(s => s.ativo == "1").ToList();
        }

        public class Sku : IFieldSku
        {
            public string sku { get; set; }
            public int cor { get; set; }
            public int estampa { get; set; }
            public string tamanho { get; set; }
            public string ativo { get; set; }
            public int? estoque_min { get; set; }
            public decimal? largura { get; set; }
            public decimal? altura { get; set; }
            public decimal? comprimento { get; set; }
            public decimal? peso { get; set; }
            public string permite_pedido_sem_estoque { get; set; }
            public int? lead_time_entrega { get; set; }
            public string pre_venda { get; set; }
            public string vitrine_produto_sku { get; set; }
            public string desc_cor { get; set; }
            public string desc_estampa { get; set; }
            public string data_alteracao { get; set; }
            public string barra { get; set; }
            public int produto { get; set; }
            public string desc_produto { get; set; }
            public bool? kit { get; set; }
            public List<ComponentesKit> componentes_sku_kit { get; set; } = new List<ComponentesKit>();
            public string id { get; set; }
            public string cod_produto { get; set; }
            public string desc_tamanho { get; set; }
            public string barra13 { get; set; }
            public bool? importado { get; set; }
            public bool? sku_padrao { get; set; }
            public string referencia { get; set; }
            public string barra_ref { get; set; }
            public bool? sku_base { get; set; }
            public string obs_sku1 { get; set; }
            public string obs_sku2 { get; set; }
            public string obs_sku3 { get; set; }
            public string obs_sku4 { get; set; }
            public string obs_sku5 { get; set; }
            public string url_imagens { get; set; }
            public string codigo_imagem { get; set; }
            public string observacao_cor { get; set; }
            public int vitrine { get; set; }
            public decimal? preco1 { get; set; }
            public decimal? preco2 { get; set; }
            public string id_externo { get; set; }

        }

        public class Classificaco
        {
            public int? vitrine_classificacao { get; set; }
            public string desc_classificacao { get; set; }
            public bool excluido { get; set; }
            public string arvore_classificacao { get; set; }
            public string id { get; set; }
            public bool? pickup_in_store { get; set; }
            public bool? mandatory_for_lookbook { get; set; }
            public bool? visivel_web { get; set; }
            public bool? permite_presente { get; set; }
        }

        public class ComponentesKit
        {
            public int produto_pai { get; set; }
            public int produto_filho { get; set; }
            public int quantidade { get; set; }
            public string sku { get; set; }
            public string cod_produto { get; set; }
        }
    }
}
