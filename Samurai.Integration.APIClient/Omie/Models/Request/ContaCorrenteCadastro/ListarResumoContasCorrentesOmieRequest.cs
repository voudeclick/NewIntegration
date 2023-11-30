
using Samurai.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.ContaCorrenteCadastro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.ContaCorrenteCadastro
{
    public class ListarResumoContasCorrentesOmieRequest : BaseContaCorrenteCadastroOmieRequest<ListarResumoContasCorrentesOmieRequestInput, ListarResumoContasCorrentesOmieRequestOutput>
    {
        public override string Method => "ListarResumoContasCorrentes";

        public ListarResumoContasCorrentesOmieRequest(ListarResumoContasCorrentesOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
