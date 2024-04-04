namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public abstract class BaseMutation<I, O>
        where I : BaseMutationInput
        where O : BaseMutationOutput, new()
    {
        public BaseMutation(I variables)
        {
            Variables = variables;
        }
        public abstract string GetQuery();

        public virtual string ApiVersion => null;

        public I Variables { get; set; }
    }

    public abstract class BaseMutationInput
    {
    }

    public abstract class BaseMutationOutput
    {
    }
}
