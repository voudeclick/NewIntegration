using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Samurai.Integration.Application.Services;
using Microsoft.AspNetCore.Authorization;

namespace Samurai.Integration.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrador,Suporte,Viewer")]
    public class MilleniumController : ControllerBase
    {
        private readonly ILogger<MilleniumController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TenantRepository _tenantRepository;
        private readonly TenantService _tenantService;
        //private readonly CancellationToken _cancellationToken;

        public MilleniumController(ILogger<MilleniumController> logger,
                                IHttpClientFactory httpClientFactory,
                                TenantRepository tenantRepository,
                                TenantService tenantService)/*,
                                CancellationToken cancellationToken)*/
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _tenantRepository = tenantRepository;
            _tenantService = tenantService;
            //_cancellationToken = cancellationToken;
        }

        [HttpGet("GetTrackingCode")]
        public async Task<string> GetTrackingCode(string IdOrderMillenium, string shopifyStoreDomain, CancellationToken cancellationToken)
        {
            try
            {
                var tenant = await _tenantRepository.GetTenantWithShopifyData(shopifyStoreDomain.Split('.')[0].Trim());

                var trackingCode = await _tenantService.GetTrackingCodeFromOrder(tenant, IdOrderMillenium, cancellationToken);

                return trackingCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error on MillenniumController GetTrackingCode");
            }

            return null;
        }
    }
}
