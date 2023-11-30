using Samurai.Integration.Domain.Results;

namespace Samurai.Integration.WebApi.ViewModels
{
    public abstract class BaseViewModel
    {
        public abstract Result IsValid();
    }
}
