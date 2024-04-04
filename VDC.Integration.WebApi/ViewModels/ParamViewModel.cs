using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using VDC.Integration.Domain.Results;

namespace VDC.Integration.WebApi.ViewModels
{
    public class ParamViewModel : BaseViewModel
    {
        public string Key { get; set; }

        public List<ParamValueViewModel> Values { get; set; } = new List<ParamValueViewModel>();

        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(Key))
            {
                result.AddError("Requerido", "Informe a Key.", GetType().FullName);
            }

            if (!Values.Any())
            {
                result.AddError("Requerido", "Informe ao menos um par de valores.", GetType().FullName);
            }

            if (result.IsFailure)
            {
                result.StatusCode = HttpStatusCode.BadRequest;
            }

            return result;
        }
    }

    public class ParamValueViewModel
    {
        public string Key { get; set; }

        public JsonElement Value { get; set; }

    }
}
