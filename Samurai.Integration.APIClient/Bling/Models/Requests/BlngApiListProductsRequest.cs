using System;

namespace Samurai.Integration.APIClient.Bling.Models.Requests
{
    public class BlngApiListProductsRequest
    {
        public DateTime? ProductUpdatedDate { get; set; }
        public string ProductCode { get; set; }
        public string CategoriaId { get; set; }

    }
}
