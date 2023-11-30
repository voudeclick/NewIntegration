using Samurai.Integration.APIClient.Omie.Models.Request.Inputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ClienteCadastro.Inputs
{
    public class UpsertClienteOmieRequestInput : BaseOmieInput
    {
        public long? codigo_cliente_omie { get; set; }
        public string codigo_cliente_integracao { get; set; }
        public string razao_social { get; set; }
        public string cnpj_cpf { get; set; }
        public string email { get; set; }
        public string nome_fantasia { get; set; }
        public string estado { get; set; }
        public string cidade { get; set; }
        public string cep { get; set; }
        public string bairro { get; set; }
        public string endereco { get; set; }
        public string endereco_numero { get; set; }
        public string complemento { get; set; }
        public string telefone1_ddd { get; set; }
        public string telefone1_numero { get; set; }
        public List<Tag> tags { get; set; }
    }
}
