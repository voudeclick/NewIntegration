using Samurai.Integration.Domain.Messages.Tray;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Product
{
    public class UpdateProductProcessingRequest
    {
        public TrayAppReturnMessage TrayAppMessage { get; set; }
    }
}
