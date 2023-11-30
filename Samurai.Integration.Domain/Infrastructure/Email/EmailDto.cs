using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Infrastructure.Email
{
    public class EmailDto
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
