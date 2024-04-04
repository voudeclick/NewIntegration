using VDC.Integration.APIClient.Omie.Models.Request;

namespace VDC.Integration.APIClient.Omie.Models.Result.FamiliaCadastro
{
    public class ConsultarFamiliaOmieRequestOutput : BaseOmieOutput
    {
        public long codigo { get; set; }
        public string codInt { get; set; }
        public string codFamilia { get; set; }
        public string nomeFamilia { get; set; }
        public string inativo { get; set; }
    }
}