using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Models.Millennium
{
    public class MillenniumApiCreateOrderRequest
    {
        public int? conta { get; set; }

        public int? tipo_pedido { get; set; }

        public string cod_pedidov { get; set; } 

        public string data_emissao { get; set; }

        public string data_entrega { get; set; }

        public decimal? acerto { get; set; }

        public decimal? v_acerto { get; set; }

        public decimal total { get; set; }

        public decimal quantidade { get; set; }

        public decimal? cortesia { get; set; }

        public decimal v_frete { get; set; }

        public string origem_pedido { get; set; }

        public long vitrine { get; set; }

        public string nome_transportadora { get; set; }

        public int? transportadora { get; set; }

        public int? tipo_frete { get; set; }

        public List<MilenniumApiCreateOrderPaymentDataRequest> lancamentos { get; set; }

        public List<MilenniumApiCreateOrderProductRequest> produtos { get; set; }

        public List<MilenniumApiCreateOrderCustomerRequest> dados_cliente { get; set; }

        public string n_pedido_cliente { get; set; }

        public string obs { get; set; }

        public string obs2 { get; set; }

        public bool aprovado { get; set; }

        public List<MilenniumApiCreateOrderMessage> mensagens { get; set; }
    } 

    public class MilenniumApiCreateOrderProductRequest
    {
        public decimal quantidade { get; set; }

        public decimal preco { get; set; }

        public string obs_item { get; set; }

        public string sku { get; set; }

        public decimal? desconto { get; set; }

        public bool? brindesite { get; set; }
        public string cod_filial { get; set; }
        public string encomenda { get; set; }
        public string item { get; set; }
    }

    public class MilenniumApiCreateOrderPaymentDataRequest
    {
        public DateTime? data_vencimento { get; set; }

        public decimal valor_inicial { get; set; }

        public decimal valor_liquido { get; set; }

        public int? tipo_pgto { get; set; }

        public string documento { get; set; }

        public string numero_cartao { get; set; }

        public decimal? desconto { get; set; }

        public int numparc { get; set; }

        public int bandeira { get; set; }

        public int operadora { get; set; }

        public int parcela { get; set; }

        public string DESC_TIPO { get; set; }

        public string transacao_aprovada { get; set; }

        public string NSU { get; set; }

        public string Autorizacao { get; set; }

        public string Data_Aprova_Facil { get; set; }

        public string Duplicata { get; set; }
    }

    public class MilenniumApiCreateOrderCustomerRequest
    {
        public string cod_cliente { get; set; }

        public string nome { get; set; }

        public string ddd_cel { get; set; }

        public string cel { get; set; }

        public string rg { get; set; }

        public string data_aniversario { get; set; }

        public string obs { get; set; }

        public string e_mail { get; set; }

        public string tipo_sexo { get; set; }

        public string tratamento { get; set; }

        public bool? maladireta { get; set; }

        public string cnpj { get; set; }

        public string pf_pj { get; set; }

        public string ie { get; set; }

        public string ufie { get; set; }

        public string fantasia { get; set; }

        public string grupo_loja { get; set; }

        public int? parentesco { get; set; }

        public string cpf { get; set; }

        public long vitrine { get; set; }

        public string dados_adicionais { get; set; }

        public List<MilenniumApiCreateOrderAddressRequest> endereco { get; set; }

        public List<MilenniumApiCreateOrderAddressRequest> endereco_entrega { get; set; }
    }

    public class MilenniumApiCreateOrderAddressRequest
    {
        public string logradouro { get; set; }

        public string bairro { get; set; }

        public string cidade { get; set; }

        public string estado { get; set; }

        public string cep { get; set; }

        public string ddd { get; set; }

        public string fone { get; set; }

        public string ramal { get; set; }

        public string fax { get; set; }

        public string contato { get; set; }

        public string dicas_endereco { get; set; }

        public string complemento { get; set; }

        public string numero { get; set; }

        public string ddd_cel { get; set; }

        public string nome_pais { get; set; }

        public string desc_tipo_endereco { get; set; }

        public string tipo_sexo { get; set; }

        public string grau_relacionamento { get; set; }

        public string cod_filial_retira { get; set; }

        public string cnpj_filial_retira { get; set; }

        public long? cod_endereco { get; set; }
    }

    public class MilenniumApiCreateOrderMessage
    {
        public string texto { get; set; }
    }
}
