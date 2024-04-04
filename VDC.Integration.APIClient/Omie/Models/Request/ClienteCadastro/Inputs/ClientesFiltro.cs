using System.Collections.Generic;
using VDC.Integration.APIClient.Omie.Models.Request.Inputs;

namespace VDC.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs
{
    public class ClientesFiltro
    {
        public int? codigo_cliente_omie { get; set; }
        public string codigo_cliente_integracao { get; set; }
        public string cnpj_cpf { get; set; }
        public string razao_social { get; set; }
        public string nome_fantasia { get; set; }
        public string endereco { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string cep { get; set; }
        public string contato { get; set; }
        public string email { get; set; }
        public string inscricao_municipal { get; set; }
        public string inscricao_estadual { get; set; }
        public List<Tag> tags { get; set; }
    }
}
