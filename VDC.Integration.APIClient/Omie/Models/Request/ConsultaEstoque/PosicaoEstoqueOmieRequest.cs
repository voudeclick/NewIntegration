using VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque.Inputs;
using VDC.Integration.APIClient.Omie.Models.Result.ConsultaEstoque;

namespace VDC.Integration.APIClient.Omie.Models.Request.ConsultaEstoque
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
