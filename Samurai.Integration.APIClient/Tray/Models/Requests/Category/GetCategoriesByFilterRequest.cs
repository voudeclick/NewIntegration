using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Category
{
    public class GetCategoriesByFilterRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Limit { get => 100; }
        public int Page { get => 1; }
    }
}
