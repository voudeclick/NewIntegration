namespace Samurai.Integration.APIClient.Shopify.Models.Request
{
    public abstract class BaseQuery<O>
        where O : BaseQueryOutput, new()
    {
        public abstract string GetQuery();

        public virtual string ApiVersion => null;

        protected string GetCursor(string cursor)
        {
            return cursor != null ?
                $", after: \"{cursor}\"" : "";
        }
    }

    public abstract class BaseQueryOutput
    {
    }
}
