
using Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.FamiliaCadastro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.FamiliaCadastro
{
    public class ConsultarFamiliaOmieRequest : BaseFamiliaCadastroOmieRequest<ConsultarFamiliaOmieRequestInput, ConsultarFamiliaOmieRequestOutput>
    {
        public override string Method => "ConsultarFamilia";

        public ConsultarFamiliaOmieRequest(ConsultarFamiliaOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
