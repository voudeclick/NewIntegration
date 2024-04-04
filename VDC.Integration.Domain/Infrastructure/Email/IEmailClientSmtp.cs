using System.Threading.Tasks;

namespace VDC.Integration.Domain.Infrastructure.Email
{
    public interface IEmailClientSmtp
    {
        Task SendAsync(EmailDto emailDto);
    }
}
