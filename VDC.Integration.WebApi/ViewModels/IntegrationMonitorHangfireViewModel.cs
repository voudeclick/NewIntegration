using VDC.Integration.Domain.Results;

namespace VDC.Integration.WebApi.ViewModels
{
    public class IntegrationMonitorHangfireViewModel : BaseViewModel
    {
        public string CronExpression { get; set; }
        public override Result IsValid()
        {
            var result = new Result() { StatusCode = System.Net.HttpStatusCode.OK };

            if (string.IsNullOrEmpty(CronExpression))
            {
                result.AddError("Requerido", "Informe a CronExpression", GetType().FullName);
                result.StatusCode = System.Net.HttpStatusCode.BadRequest;
            }

            return result;
        }
    }
}
