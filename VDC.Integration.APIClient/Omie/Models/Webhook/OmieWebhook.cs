namespace VDC.Integration.APIClient.Omie.Models.Webhook
{
    public class OmieWebhook
    {
        public string ping { get; set; }

        public string messageId { get; set; }
        public string topic { get; set; }
        public Event @event { get; set; }
        public Author author { get; set; }
        public string appKey { get; set; }
        public string appHash { get; set; }
        public string origin { get; set; }

        public class Event
        {
            public long? codigo_familia { get; set; }
            public string codigo { get; set; }
            public long? codigo_produto { get; set; }
            public string inativo { get; set; }
            public string codIntPedido { get; set; }
        }


        public class Author
        {
            public string email { get; set; }
            public string name { get; set; }
            public long? userId { get; set; }
        }
    }

}
