
using Samurai.Integration.APIClient.Omie.Models.Request.ConsultaEstoque.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ConsultaEstoque;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ConsultaEstoque
{
    public class PosicaoEstoqueOmieRequest : BaseConsultaEstoqueOmieRequest<PosicaoEstoqueOmieRequestInput, PosicaoEstoqueOmieRequestOutput>
    {
        public override string Method => "PosicaoEstoque";

        public PosicaoEstoqueOmieRequest(PosicaoEstoqueOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
