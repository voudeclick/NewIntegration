namespace VDC.Integration.APIClient.Shopify.Models.Request.Inputs
{
    public class Metafield
    {
        public string id { get; set; }
        public string key { get; set; }
        public string value { get; set; }
        public string @namespace { get { return "VDC.Integration"; } }
        public string valueType { get; set; }
    }
}
