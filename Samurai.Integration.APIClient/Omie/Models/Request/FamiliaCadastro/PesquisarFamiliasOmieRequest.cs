
using Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro
{
    public class PesquisarFamiliasOmieRequest : BaseFamiliaCadastroOmieRequest<PesquisarFamiliasOmieRequestInput, PesquisarFamiliasOmieRequestOutput>
    {
        public override string Method => "PesquisarFamilias";

        public PesquisarFamiliasOmieRequest(PesquisarFamiliasOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
