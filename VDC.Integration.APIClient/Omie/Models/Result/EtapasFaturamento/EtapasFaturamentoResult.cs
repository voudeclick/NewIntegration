using System.Collections.Generic;

namespace VDC.Integration.APIClient.Omie.Models.Result.EtapasFaturamento
{
    public class EtapasFaturamentoResult
    {
        public string cCodOperacao { get; set; }
        public string cDescOperacao { get; set; }
        public List<Etapa> etapas { get; set; }

        public class Etapa
        {
            public string cCodigo { get; set; }
            public string cDescrPadrao { get; set; }
            public string cDescricao { get; set; }
        }
    }
}