﻿using Akka.Event;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Samurai.Integration.APIClient.Shopify;
using Samurai.Integration.APIClient.Shopify.Models.Request;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Shopify;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Actors.API
{
    public class ShopifyApi
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;
        private readonly ShopifyApp _app;
        private readonly ShopifyApiClient _client;
        private readonly ShopifyRESTClient _restClient;
        protected readonly ILoggingAdapter _logAkka;

        protected ShopifyDataMessage _shopifyData;

        public ShopifyApi(IServiceProvider serviceProvider, CancellationToken cancellationToken, ShopifyDataMessage shopifyData, ShopifyApp app)
        {
            _serviceProvider = serviceProvider;
            _cancellationToken = cancellationToken;
            _shopifyData = shopifyData;
            _app = app;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
                var httpClientFactory = scope.ServiceProvider.GetService<IHttpClientFactory>();

                var loggerShopifyApiClient = scope.ServiceProvider.GetService<ILogger<ShopifyApiClient>>();
                var loggerShopifyRESTClient = scope.ServiceProvider.GetService<ILogger<ShopifyRESTClient>>();


                string versionShopify = shopifyData.Id == 57 ? 
                    _configuration.GetSection("Shopify")["NewVersion"] : _configuration.GetSection("Shopify")["Version"];

                _client = new ShopifyApiClient(httpClientFactory, shopifyData.Id.ToString(), _shopifyData.ShopifyStoreDomain, versionShopify, _app.ShopifyPassword);
                _restClient = new ShopifyRESTClient(loggerShopifyRESTClient, httpClientFactory, shopifyData.Id.ToString(), _shopifyData.ShopifyStoreDomain, versionShopify, _app.ShopifyPassword);
            }

        }


        public async Task<ReturnMessage<O>> Receive<O>(BaseQuery<O> message)
           where O : BaseQueryOutput, new()
        {
            try
            {
                
                var response = await _client.Post(message, _cancellationToken);
                return new ReturnMessage<O> { Result = Result.OK, Data = response };
            }
            catch (Exception ex)
            {
                return new ReturnMessage<O> { Result = Result.Error, Error = ex };
            }
        }

    }
}