using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Models.Pier8
{
    public class Destinatario
    {
        public string cnpjcpf { get; set; }
        public string nome { get; set; }
        public string logradouro { get; set; }
        public string numero { get; set; }
        public string compl { get; set; }
        public string referencia { get; set; }
        public string bairro { get; set; }
        public long? cep { get; set; }
        public string fone { get; set; }
        public string email { get; set; }
        public string cartaopresente { get; set; }
        public string listapresente { get; set; }
        public string pack { get; set; }
        public string voucherreversa { get; set; }

    }
}
