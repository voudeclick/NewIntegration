using System.Collections.Generic;

namespace Samurai.Integration.APIClient.Millennium.Models.Results
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
