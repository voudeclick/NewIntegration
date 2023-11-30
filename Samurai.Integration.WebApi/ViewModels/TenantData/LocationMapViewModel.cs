using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.Domain.Models;
using Samurai.Integration.Domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ViewModels.TenantData
{
    public class LocationMapViewModel : BaseViewModel
    {
        public string JsonMap { get; set; }
        public List<LocationViewModel> Locations { get; set; } = new List<LocationViewModel>();


        public override Result IsValid()
        {
            var result = new Result { StatusCode = HttpStatusCode.OK };

            if (string.IsNullOrWhiteSpace(JsonMap))
                result.AddError("Operação inválida", "Origin inválido.", GetType().FullName);


            return result;
        }
        public LocationMapViewModel()
        {

        }
        public LocationMapViewModel(LocationMap data)
        {
            Locations = data.GetLocations()?.Select(x => new LocationViewModel(x)).ToList();
            JsonMap = data.JsonMap;
        }

        public class LocationViewModel : BaseViewModel
        {
            public string ErpLocation { get; set; }
            public string EcommerceLocation { get; set; }

            public override Result IsValid()
            {
                var result = new Result { StatusCode = HttpStatusCode.OK };

                if (string.IsNullOrWhiteSpace(ErpLocation))
                    result.AddError("Operação inválida", "ErpLocation inválido.", GetType().FullName);

                if (string.IsNullOrWhiteSpace(EcommerceLocation))
                    result.AddError("Operação inválida", "EcommerceLocation inválida.", GetType().FullName);

                return result;
            }
            public LocationViewModel()
            {
            }

            public LocationViewModel(LocationItem entity)
            {
                ErpLocation = entity.ErpLocation;
                EcommerceLocation = entity.EcommerceLocation;
            }
        }
    }
    public static class LocationMapViewModelExtensions
    {
        public static void UpdateFrom(this LocationMap entity, LocationMapViewModel viewModel)
        {

            entity.SetLocations(viewModel.Locations.Select(x => new LocationItem { ErpLocation = x.ErpLocation, EcommerceLocation = x.EcommerceLocation }).ToList());

        }
    }
}
