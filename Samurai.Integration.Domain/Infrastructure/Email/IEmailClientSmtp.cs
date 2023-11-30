using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Infrastructure.Email
{
    public interface IEmailClientSmtp
    {
        Task SendAsync(EmailDto emailDto);
    }
}
