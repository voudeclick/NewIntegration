using System.Collections.Generic;

namespace VDC.Integration.Domain.Infrastructure.Email
{
    public class EmailDto
    {
        public List<string> ToEmails { get; set; } = new List<string>();
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
