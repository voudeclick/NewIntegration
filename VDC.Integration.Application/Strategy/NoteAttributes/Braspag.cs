using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VDC.Integration.Application.Services;
using VDC.Integration.Application.Strategy.Interfaces;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Dtos;

namespace VDC.Integration.Application.Strategy.NoteAttributes
{
    public class Braspag : IValidationGateway
    {
        private readonly ShopifyService _shopifyService;
        private readonly IServiceProvider _serviceProvider;

        public Braspag(IServiceProvider shopifyService)
        {
            _serviceProvider = shopifyService;
            _shopifyService = _serviceProvider.GetService<ShopifyService>();
        }

        public bool IsGateway(string gateways) => gateways == Gateways.Cielo;

        public async Task<List<NoteAttributeDto>> GetNoteAttributes(string checkoutId)
            => await _shopifyService.ReturnNoteAttributes(Gateways.BaseCielo, Gateways.MethodCielo, new Dictionary<string, string>() { { "IdCheckout", checkoutId } });
    }
}
