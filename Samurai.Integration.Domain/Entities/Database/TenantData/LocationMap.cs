using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Samurai.Integration.Domain.Entities.Database.TenantData
{
    public class LocationMap : BaseEntity
    {
        public string JsonMap { get; private set; }

        public List<LocationItem> GetLocations()
        {
            if (string.IsNullOrWhiteSpace(JsonMap))
                return new List<LocationItem>();
            return JsonSerializer.Deserialize<List<LocationItem>>(JsonMap);
        }
        public void SetLocations(List<LocationItem> json)
        {
            JsonMap = JsonSerializer.Serialize(json);
        }

        public LocationItem GetLocationByIdErp(string location) =>
            GetLocations().Where(x => x.ErpLocation?.ToLower()?.Trim() == location?.ToLower()?.Trim()).FirstOrDefault();
        public LocationItem GetLocationByIdEcommerce(string location) =>
            GetLocations().Where(x => x.EcommerceLocation?.ToLower()?.Trim() == location?.ToLower()?.Trim()).FirstOrDefault();



    }
}
