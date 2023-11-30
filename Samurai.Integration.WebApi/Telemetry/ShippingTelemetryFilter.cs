using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.Telemetry
{
    public class ShippingTelemetryFilter : ITelemetryProcessor
    {
        private ITelemetryProcessor Next { get; set; }

        // next will point to the next TelemetryProcessor in the chain.
        public ShippingTelemetryFilter(ITelemetryProcessor next)
        {
            this.Next = next;
        }

        public void Process(ITelemetry item)
        {
            // To filter out an item, return without calling the next processor.
            if (IsShippingCalculate(item)) 
                return; 

            this.Next.Process(item);
        }

        private bool IsShippingCalculate(ITelemetry item)
        {
            return item.Context?.Operation?.Name?.Contains("Shipping/Calculate") == true;
        }
    }
}
