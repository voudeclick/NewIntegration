using VDC.Integration.Domain.Results;

namespace VDC.Integration.WebApi.ViewModels
{
    public abstract class BaseViewModel
    {
        public abstract Result IsValid();
    }
}
