using System.Collections.Generic;
using VDC.Integration.APIClient.Shopify.Models.Request.Inputs;
using VDC.Integration.Domain.Shopify.Models.Results;

namespace VDC.Integration.APIClient.Shopify.Models.Request
{
    public class CollectionCreateMutation : BaseMutation<CollectionCreateMutationInput, CollectionCreateMutationOutput>
    {
        public CollectionCreateMutation(CollectionCreateMutationInput variables)
            : base(variables)
        {
        }

        public override string GetQuery()
        {
            return $@"
                mutation collectionCreate($input: CollectionInput!) {{
                  collectionCreate(input: $input) {{
                    userErrors {{
                        field
                        message
                    }}
                    collection {{
                        id,
                        title,
                        ruleSet {{
                            appliedDisjunctively,
                            rules {{
                                column,
                                condition,
                                relation
                            }}
                        }}
                    }}
                  }}
                }}
            ";
        }
    }

    public class CollectionCreateMutationInput : BaseMutationInput
    {
        public Collection input { get; set; }
    }

    public class CollectionCreateMutationOutput : BaseMutationOutput
    {
        public Result collectionCreate { get; set; }

        public class Result
        {
            public CollectionResult collection { get; set; }
            public List<UserError> userErrors { get; set; }
        }
    }
}
