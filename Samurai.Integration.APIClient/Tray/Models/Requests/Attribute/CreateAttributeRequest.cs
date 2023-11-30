using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Tray.Models.Requests.Attribute
{
    public class CreateAttributeRequest
    {
        public string Name { get; set; }

        public List<PropertyValue> PropertyValues { get; set; }

        public class PropertyValue
        {
            public string Name { get; set; }
        }
    }
}
