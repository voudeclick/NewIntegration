namespace VDC.Integration.Domain.Queues
{
    public interface IBaseQueue
    {
        long Id { get; set; }
        string StoreHandle { get; set; }
    }
}
