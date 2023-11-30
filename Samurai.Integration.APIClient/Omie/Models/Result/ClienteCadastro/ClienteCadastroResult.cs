namespace Samurai.Integration.APIClient.Omie.Models.Result.ClienteCadastro
{
    public class ClienteCadastroResult
    {
        public int codigo_cliente_omie { get; set; }
        public string codigo_cliente_integracao { get; set; }
        public string bairro { get; set; }
        public string cep { get; set; }
        public string cidade { get; set; }
        public string cidade_ibge { get; set; }
        public string cnpj_cpf { get; set; }
        public string codigo_pais { get; set; }
        public string complemento { get; set; }
        public string contato { get; set; }
        public string email { get; set; }
        public string endereco { get; set; }
        public string endereco_numero { get; set; }
        public string estado { get; set; }
        public string exterior { get; set; }
        public string inativo { get; set; }
        public string inscricao_estadual { get; set; }
        public string inscricao_municipal { get; set; }
        public string nome_fantasia { get; set; }
        public string observacao { get; set; }
        public string pessoa_fisica { get; set; }
        public string razao_social { get; set; }
        public Tag[] tags { get; set; }
        public string telefone1_ddd { get; set; }
        public string telefone1_numero { get; set; }

        public class Tag
        {
            public string tag { get; set; }
        }
    }
}