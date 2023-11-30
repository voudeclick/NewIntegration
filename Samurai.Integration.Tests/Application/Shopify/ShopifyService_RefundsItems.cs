using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Tests.Mock.Shopify;

namespace Samurai.Integration.Tests.Application.Millennium
{
    [TestFixture]
    public class ShopifyService_RefundsItems
    {
        private ShopifyService _shopifyService;

        [SetUp]
        public void Setup()
        {
            var service = new ServiceCollection();  
            service.AddTransient<ShopifyService, ShopifyService>(); 

            var provider = service.BuildServiceProvider();
            _shopifyService = provider.GetService<ShopifyService>();            
        }

        [Test]
        public void RefundsItems_Order()
        {
            var orderMock = new ShopifyOrderMock();

            var result = _shopifyService.RefundsItems(orderMock.order.order);
           
            Assert.AreEqual(0,result.line_items.Count);
        }      
    }
}