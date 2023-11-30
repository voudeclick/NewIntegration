using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Category
{
    public class CreateCategoryRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public string Order { get; set; }
        public string Title { get; set; }

        [JsonProperty("small_description")]
        public string SmallDescription { get; set; }

        [JsonProperty("has_acceptance_term")]
        public string HasAcceptanceTerm { get; set; }

        [JsonProperty("acceptance_term")]
        public string AcceptanceTerm { get; set; }

        public MetatagCategoryModel Metatag { get; set; }
        public List<string> Property { get; set; }

        public class MetatagCategoryModel
        {
            public string Keywords { get; set; }
            public string Description { get; set; }
        }
    }
}
