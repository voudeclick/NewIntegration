
namespace VDC.Integration.Domain.Messages.ServiceBus
{
    public class IntegrationSettingsMessage
    {
        public string OriginMessageTypeFullName { get; set; }
        public string DestinyMessageTypeFullName { get; set; }
    }
}
