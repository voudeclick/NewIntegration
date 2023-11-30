

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Tests.Mock.Millennium;
using System;

namespace Samurai.Integration.Tests.Application.Millennium
{
    [TestFixture]
    public class MillenniumService_GetBandeira
    {
        private MillenniumService _millenniumService;

        [SetUp]
        public void Setup()
        {
            var service = new ServiceCollection();  
            service.AddTransient<MillenniumService, MillenniumService>(); 

            var provider = service.BuildServiceProvider();
            _millenniumService = provider.GetService<MillenniumService>();            
        }

        [Test]
        public void GetBandeira_MillenniumIssuerType()
        {
            var orderMock = new MillenniumOrderMock();
            
            var result = _millenniumService.GetBandeira(orderMock.order);
            
            Assert.NotNull(result);            
            Assert.That(Enum.IsDefined(typeof(MillenniumIssuerType), result));
        }      
    }
}