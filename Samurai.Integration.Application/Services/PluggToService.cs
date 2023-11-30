using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Samurai.Integration.APIClient.PluggTo.Models.Requests;
using Samurai.Integration.APIClient.PluggTo.Models.Results;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.PluggTo;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class PluggToService
    {
        private ILoggingAdapter _logger;
        private readonly IMapper _mapper;

        private IActorRef _apiActorGroup;

        public PluggToService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;

        }

        public async Task<ReturnMessage<List<PluggToApiListProductsResult.Produto>>> ListProducts(PluggToListAllProductsMessage message, CancellationToken cancellationToken)
        {
            var productList = await _apiActorGroup.Ask<ReturnMessage<PluggToApiListProductsResult>>(
                new PluggToApiListProductRequest
                {
                    CreatedAt = message.CreatedAt,
                    AccountUserId = message.AccountUserId,
                    AccountSellerId = message.AccountSellerId
                }, cancellationToken);

            if (productList.Result == Result.Error)
                return new ReturnMessage<List<PluggToApiListProductsResult.Produto>> { Result = Result.Error, Error = productList.Error };

            return new ReturnMessage<List<PluggToApiListProductsResult.Produto>> { Data = productList.Data?.result?.Select(x => x.Product)?.ToList() };
        }

        public async Task<ReturnMessage<PluggToApiListProductsResult.Produto>> ListProduct(PluggToListProductMessage message, CancellationToken cancellationToken)
        {
            var product = await _apiActorGroup.Ask<ReturnMessage<PluggToApiListProductsResult>>(
                   new PluggToApiListProductRequest
                   {
                       ProductCode = message.Sku,
                       ExternalId = message.ExternalId,
                       AccountUserId = message.AccountUserId,
                       AccountSellerId = message.AccountSellerId
                   }, cancellationToken);

            return new ReturnMessage<PluggToApiListProductsResult.Produto> { Data = product?.Data?.result?.FirstOrDefault()?.Product };
        }
    }
}
