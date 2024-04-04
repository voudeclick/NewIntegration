using System.Collections.Generic;

namespace VDC.Integration.APIClient.Millennium.Models.Results
{
    public class MillenniumApiBuscaFotoResult
    {
        public List<Value> value { get; set; }
        public class Value
        {
            public string foto { get; set; }

        }
    }
}
