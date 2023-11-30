using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Bling.Models.Webhook
{
    public class BlingStockUpdateModel
    {
        public Retorno retorno { get; set; }
        public class Retorno
        {
            public List<EstoqueWrapper> estoques { get; set; }
        }

        public class EstoqueWrapper
        {
            public Estoque estoque { get; set; }
        }

        public class Estoque
        {
            public string codigo { get; set; }
            public string nome { get; set; }
            public decimal estoqueAtual { get; set; }
            public List<DepositoWrapper> depositos { get; set; }
        }

        public class DepositoWrapper
        {
            public Deposito deposito { get; set; }
        }

        public class Deposito
        {
            public string id { get; set; }
            public string nome { get; set; }
            public string saldo { get; set; }
            public string desconsiderar { get; set; }
            public string saldoVirtual { get; set; }
        }
    }  
}
