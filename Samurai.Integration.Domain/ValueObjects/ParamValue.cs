using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Samurai.Integration.Domain.ValueObjects
{
    public class ParamValue
    {
        public string Key { get; set; }
        public object Value { get; set; } 
        public double GetDoubleOrDefault(double @default = 0)
        {
            if(double.TryParse(Value.ToString(), out double @double))
            {
                return @double;
            }

            return @default;
        }
    }
}
