namespace Samurai.Integration.Domain.Messages.Nexaas
{
    public class NexaasListOrderMessage
    {
        public long? NexaasOrderId { get; set; }
        public string ExternalOrderId { get; set; }
    }
}
