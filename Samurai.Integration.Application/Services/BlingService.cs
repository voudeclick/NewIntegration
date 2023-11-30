using Akka.Actor;
using Akka.Event;
using AutoMapper;
using Samurai.Integration.APIClient.Bling.Models.Requests;
using Samurai.Integration.APIClient.Bling.Models.Results;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Bling;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Samurai.Integration.Application.Services
{
    public class BlingService
    {
        private ILoggingAdapter _logger;
        private readonly TenantRepository _tenantRepository;
        private IActorRef _apiActorGroup;
        private readonly IMapper _mapper;
        private BlingData _blingData;

        public BlingService(TenantRepository tenantRepository, IMapper mapper)
        {
            _mapper = mapper;
            _tenantRepository = tenantRepository;
        }

        public void Init(IActorRef apiActorGroup, ILoggingAdapter logger, BlingData blingData)
        {
            _apiActorGroup = apiActorGroup;
            _logger = logger;
            _blingData = blingData;
        }

        public async Task<ReturnMessage<List<BlingApiListProductsResult.Produto>>> ListProducts(DateTime? productUpdatedDate, CancellationToken cancellationToken = default)
        {
            var productList = await _apiActorGroup.Ask<ReturnMessage<BlingApiListProductsResult>>(
                new BlngApiListProductsRequest {
                    ProductUpdatedDate = productUpdatedDate ,
                    CategoriaId = _blingData.CategoriaId 
                }, cancellationToken
            );

            if (productList.Result == Result.Error)
                return new ReturnMessage<List<BlingApiListProductsResult.Produto>> { Result = Result.Error, Error = productList.Error };

            return new ReturnMessage<List<BlingApiListProductsResult.Produto>> { Data = productList.Data?.retorno?.produtos?.Select(x => x.produto)?.ToList() };
        }

        public async Task<ReturnMessage<BlingApiListProductsResult.Produto>> ListProduct(string productCode, CancellationToken cancellationToken)
        {
            var product = await _apiActorGroup.Ask<ReturnMessage<BlingApiListProductsResult>>(
                   new BlngApiListProductsRequest {
                       ProductCode = productCode,
                       CategoriaId = _blingData.CategoriaId
                   }, cancellationToken
            );

            return new ReturnMessage<BlingApiListProductsResult.Produto> { Data = product?.Data?.retorno?.produtos?.FirstOrDefault()?.produto };
        }
    }
}
