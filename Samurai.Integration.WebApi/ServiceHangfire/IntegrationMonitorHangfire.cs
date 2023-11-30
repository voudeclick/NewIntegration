using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NCrontab;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Consts;
using Samurai.Integration.Domain.Dtos;
using Samurai.Integration.Domain.Enums;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.WebApi.ServiceHangfire
{
    public class IntegrationMonitorHangfire
    {
        private readonly ILogger<IntegrationMonitorHangfire> _logger;
        private readonly EmailService _emailService;
        private readonly TenantRepository _tenantRepository;
        private readonly ShopifyService _shopifyService;
        private readonly ParamRepository _paramRepository;
        public IntegrationMonitorHangfire(ILogger<IntegrationMonitorHangfire> logger,
            EmailService emailService,
            TenantRepository tenantRepository,
            ShopifyService shopifyService,
            ParamRepository paramRepository)
        {
            _logger = logger;
            _emailService = emailService;
            _tenantRepository = tenantRepository;
            _shopifyService = shopifyService;
            _paramRepository = paramRepository;
        }

        public async Task Initialize()
        {   
            _logger.LogInformation("Inicialização monitor de integrações de pedidos.");

            var tenants = await _tenantRepository.GetActiveOrderIntegrationByIntegrationType(IntegrationType.Shopify);

            if (!tenants.Any()) return;

            var storesLostedOrders = await _shopifyService.GetLostOrders(tenants);            

            if (!storesLostedOrders.Any()) return;

            var intervalMinutes = await CalculateMinutesInterval();

            var toleranceMinutes = _paramRepository
                                    .GetByKeyAsync(ParamConsts.SeachLostedOrders)
                                    .Result
                                    ?.GetValueBykey(SeachLostedOrdersConsts.ToleranceMinutes)
                                    ?.GetDoubleOrDefault() ?? 0.0;

            var beginDate = DateTime.UtcNow.AddMinutes(-(toleranceMinutes+intervalMinutes));

            var lostedOrders = storesLostedOrders.SelectMany(x => x.Orders);

            bool newestLostedOrdersFilter(OrderDto x) => x.CreatedAt > beginDate;

            var newestLostedOrders = lostedOrders.Where(newestLostedOrdersFilter);

            if (!newestLostedOrders.Any()) return;

            var hasLostedOrdersNotResolvedYet = newestLostedOrders.Count() != lostedOrders.Count();

            if (hasLostedOrdersNotResolvedYet)
            {
                storesLostedOrders = storesLostedOrders.Where(x => x.Orders.Any(order => newestLostedOrders.Contains(order)))                                                       
                                                       .ToList();

                storesLostedOrders.ForEach(x => x.Orders = x.Orders.Where(newestLostedOrdersFilter).ToList());
            }

            await _emailService.SendLostedOrdersToSamuraiTeamAsync(storesLostedOrders,hasLostedOrdersNotResolvedYet);

            _logger.LogInformation("Finalização monitor de integrações de pedidos.");
        }


        private async Task<double> CalculateMinutesInterval(double defaultIntervalMinutes=120)
        {            
            var paramValue = (await _paramRepository.GetByIntegrationMonitorHangfireKeyAsync())
                           ?.GetValueBykey(IntegrationMonitorHangfireConsts.CronExpression);

            if (paramValue == null)
            {
                return defaultIntervalMinutes;
            }

            var schedule = CrontabSchedule.TryParse(paramValue.Value.ToString());

            if(schedule == null)
            {
                return defaultIntervalMinutes;
            }

            var ocurrencesNumber = 2;

            var nextOcurrences = schedule.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddMonths(1))
                                         .Take(ocurrencesNumber);

            if(nextOcurrences.Count() < ocurrencesNumber)
            {
                return defaultIntervalMinutes;
            }

            var firstOcorrence = nextOcurrences.FirstOrDefault();
            var lastOcorrence = nextOcurrences.LastOrDefault();

            var result = (lastOcorrence - firstOcorrence).TotalMinutes;

            if(result <= 0)
            {
                return defaultIntervalMinutes;
            }

            return result;
        }
    }
}
