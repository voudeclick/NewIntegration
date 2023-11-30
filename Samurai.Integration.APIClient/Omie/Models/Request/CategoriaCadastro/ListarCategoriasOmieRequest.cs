
using Samurai.Integration.APIClient.Omie.Models.Request.CategoriaCadastro.Inputs;
using Samurai.Integration.APIClient.Omie.Models.Result.CategoriaCadastro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Request.CategoriaCadastro
{
    public class ListarCategoriasOmieRequest : BaseCategoriaCadastroOmieRequest<ListarCategoriasOmieRequestInput, ListarCategoriasOmieRequestOutput>
    {
        public override string Method => "ListarCategorias";

        public ListarCategoriasOmieRequest(ListarCategoriasOmieRequestInput variables)
            : base(variables)
        {
        }
    }
}
