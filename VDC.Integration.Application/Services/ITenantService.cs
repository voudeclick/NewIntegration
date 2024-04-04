using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VDC.Integration.APIClient.Shopify.Models.Request;
using VDC.Integration.Domain.Entities.Database;
using VDC.Integration.Domain.Entities.Database.MethodPayment;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Application.Services
{
    public interface ITenantService
    {
        Task CreateConfigPaymentTypeAsync(Tenant tenant);
        Task CreateQueue(Tenant tenant, string queueName);
        Task DeleteQueue(Tenant tenant, string queueName, bool sbDefault);
        Task CreateQueues(Tenant tenant);
        Task CreateWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant);
        Task DeleteWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant);
        Task EnqueueMessageShopifyOrder(Tenant tenant, long orderId, string type);
        IntegrationType ErpType(long id);
        Task<List<MethodPayment>> GetConfigPaymentTypeAsync(Tenant tenant);
        QueueClient GetQueueClient<T>(T tenant, string queueName, bool sbDefault = true) where T : IBaseQueue;
        string GetQueueName<T>(T tenant, string queueName) where T : IBaseQueue;
        Task<List<QueueCount>> GetQueuesCount(Tenant tenant);
        Task<List<OfOverflow>> GetQueuesOverflow(List<Tenant> tenants);
        Task<string> GetTrackingCodeFromOrder(Tenant tenant, string IdOrderMillenium, CancellationToken cancellationToken);
        Task<long> GetTransIdListaVitrine<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken);
        Task<long> GetTransIdPrecoDeTabela<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken);
        Task<long> GetTransIdSaldoDeEstoque<T>(Tenant tenant, string method, DateTime dateUpdate, TransIdType type, CancellationToken cancellationToken);
        Task<WebhookQueryOutput> QueryWebhooks(IHttpClientFactory httpClientFactory, Tenant tenant);
        Task SaveTenant(Tenant tenant);
        Task SendUpdateTenantMessages(Tenant tenant, TenantType? oldTenantType = null);
        Task UpdateCarrierService(IHttpClientFactory httpClientFactory, Tenant tenant);
        Task UpdateConfigPaymentTypeAsync(Tenant tenant);
        Task UpdateTenant(Tenant tenant);
    }
}