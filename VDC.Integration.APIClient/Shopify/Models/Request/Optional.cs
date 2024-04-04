namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class Optional<T>
    {
        public Optional(T value)
        {
            this.Value = value;
        }
        public T Value { get; }
    }
}
