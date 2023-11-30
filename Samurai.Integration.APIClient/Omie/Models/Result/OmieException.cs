using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Omie.Models.Result
{
    public class OmieException : Exception
    {
        public OmieException(OmieError error)
        {
            Error = error;
        }

        public OmieException(OmieError error, string message)
            :base(message)
        {
            Error = error;
        }

        public OmieException(OmieError error, string message, Exception innerException)
            : base(message, innerException)
        {
            Error = error;
        }

        public OmieError Error { get; set; }
    }
}
