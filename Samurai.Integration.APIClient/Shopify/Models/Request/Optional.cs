using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.APIClient.Shopify.Models.Request
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
