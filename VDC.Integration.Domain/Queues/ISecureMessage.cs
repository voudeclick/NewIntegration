namespace VDC.Integration.Domain.Queues
{
    public interface ISecureMessage
    {
        public bool CanSend();
        public string ExternalMessageId { get; }
    }
}
