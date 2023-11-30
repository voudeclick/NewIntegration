using Microsoft.Extensions.DependencyInjection;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Application.Strategy.Interfaces;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Strategy.NoteAttributes
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
