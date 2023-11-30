using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Results;
using System.Net;

namespace Samurai.Integration.WebApi.ViewModels
{
    public class IntegrationErrorViewModel : BaseViewModel
    {
        public string Tag { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string MessagePattern { get; set; }
        public IntegrationErrorSource SourceId { get; set; }

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Tag))
            {
                result.AddError("Requerido", "Informe a tag.", GetType().FullName);
            }

            if (string.IsNullOrWhiteSpace(Message))
            {
                result.AddError("Requerido", "Informe a mensagem.", GetType().FullName);
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                result.AddError("Requerido", "Informe a descrição.", GetType().FullName);
            }

            if (string.IsNullOrWhiteSpace(MessagePattern))
            {
                result.AddError("Requerido", "Informe o padrão da mensagem.", GetType().FullName);
            }

            if (result.IsFailure)
            {
                result.StatusCode = HttpStatusCode.BadRequest;
            }

            return result;
        }
    }
}
