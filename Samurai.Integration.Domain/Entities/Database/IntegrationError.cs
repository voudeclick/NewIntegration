using Samurai.Integration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database
{
    public class IntegrationError
    {
        public string Tag { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string MessagePattern { get; set; }
        public IntegrationErrorSource SourceId { get; set; }


        public void Update(IntegrationError integrationError)
        {
            Message = integrationError.Message;
            SourceId = integrationError.SourceId;
            Description = integrationError.Description;
            MessagePattern = integrationError.MessagePattern;
        }

    }
}
