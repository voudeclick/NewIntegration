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
    public class Moip : IValidationGateway
    {
        private readonly ShopifyService _shopifyService;
        private readonly IServiceProvider _serviceProvider;

        public Moip(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _shopifyService = _serviceProvider.GetService<ShopifyService>();
        }

        public bool IsGateway(string gateways) => gateways == Gateways.Moip;

        public async Task<List<NoteAttributeDto>> GetNoteAttributes(string checkoutId)
            => await _shopifyService.ReturnNoteAttributes(Gateways.BaseMoip, Gateways.MethodMoip, new Dictionary<string, string>() { { "IdCheckout", checkoutId } });
    }
}
