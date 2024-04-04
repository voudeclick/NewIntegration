using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Shopify;
using VDC.Integration.Domain.Consts;
using VDC.Integration.Domain.Dtos;
using VDC.Integration.Domain.Infrastructure.Email;
using VDC.Integration.EntityFramework.Repositories;

namespace VDC.Integration.Application.Services
{
    public class EmailService
    {
        private readonly IEmailClientSmtp _emailClientSmtp;
        private readonly ShopifySettings _shopifySettings;
        private readonly IConfiguration _configuration;
        private readonly ParamRepository _paramRepository;

        public EmailService(IEmailClientSmtp emailClientSmtp,
            IOptions<ShopifySettings> shopifySettings,
            IConfiguration configuration,
            ParamRepository paramRepository)
        {
            _emailClientSmtp = emailClientSmtp;
            _shopifySettings = shopifySettings.Value;
            _configuration = configuration;
            _paramRepository = paramRepository;
        }

        public Task SendLostedOrdersToVDCTeamAsync(List<StoreLostedOrdersDto> storeslostedOrders,
            bool hasLostedOrdersNotResolvedYet = false)
        {
            if (!storeslostedOrders.Any())
            {
                return Task.CompletedTask;
            }

            var urlSentinel = _configuration.GetSection("VDCIntegrationFront")
                                            .GetSection("URL")
                                            .Value;

            var message = "<p class='destaque-1'>Lista de pedidos não integrados por loja: </p>";

            message += "<ul>";

            foreach (var storeLostedOrders in storeslostedOrders)
            {
                var storeUrl = _shopifySettings.GetStoreAdmin(storeLostedOrders.ShopifyStoreDomain);

                message += @$"<li><a href='{storeUrl}' target='_blank'>{storeLostedOrders.StoreName}</a> <span class='destaque-2'>({storeLostedOrders.TotalOrdersLosted} não integrado(s))</span>";

                message += "<ul>";

                foreach (var order in storeLostedOrders.Orders)
                {
                    var storeAdminOrderUrl = _shopifySettings.GetStoreAdminOrderUrl(storeLostedOrders.ShopifyStoreDomain, order.Id);

                    message += $"<li><a href='{storeAdminOrderUrl}' target='_blank'>{order.Id}</a></li>";
                }

                message += "</ul></li><br/>";
            }

            message += "</ul>";

            if (hasLostedOrdersNotResolvedYet)
            {
                message += "<p class='destaque-warning'><b>Atençao</b>: Existem outros pedidos não integrados que foram informados anteriormente e ainda não foram solucionados.</p>";
            }

            message += $"<div style='text-align: center;'>" +
                       $"     <a href='{urlSentinel}'>Acessar o Sentinel</a>" +
                       "</div>";

            var toEmails = _paramRepository.GetByKeyAsync(ParamConsts.IntegrationMonitorHangfire)
                                           .Result
                                           .GetValueBykey(IntegrationMonitorHangfireConsts.EmailsSendTo)
                                           .Value
                                           .ToString()
                                           .Split(';');

            return _emailClientSmtp.SendAsync(new EmailDto()
            {
                ToEmails = toEmails.ToList(),
                Subject = $"Integration - Sentinel - Pedidos não integrados {DateTime.Now:dd/MM/yyyy HH:mm}",
                Body = message,
            });
        }

    }
}
