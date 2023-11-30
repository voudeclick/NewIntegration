using Akka.Event;
using AutoMapper;
using Moq;
using Moq.AutoMock;
using Samurai.Integration.APIClient.Millennium.Models.Requests;
using Samurai.Integration.APIClient.Millennium.Models.Results;
using Samurai.Integration.Application.Actors;
using Samurai.Integration.Application.Services;
using Samurai.Integration.Domain.Entities.Database.Integrations.Millenium;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Millennium;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.Domain.Services.Interfaces;
using Samurai.Integration.EntityFramework.Repositories;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Samurai.Integration.Application.Tests.Services
{
    public class MillenniumServiceTests
    {
        private AutoMocker autoMocker;

        public MillenniumServiceTests()
        {
            autoMocker = new AutoMocker();
        }
        [Fact]
        public async Task UpdateOrder_CreateOnErp_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();


            var millenniumData = new MillenniumData()
            {
                Id = 1,
                EnableSaveIntegrationInformations = true,
            };


            var message = new ShopifySendOrderToERPMessage()
            {
                ID = 123456,
                ShopifyListOrderProcessId = Guid.NewGuid(),
                Items = new List<ShopifySendOrderItemToERPMessage>()
                {
                    new ShopifySendOrderItemToERPMessage()
                    {
                        Quantity = 1,
                        Price = 1,
                        Sku = "1",
                    }
                },
                Subtotal = 1,
                Customer = new ShopifySendOrderCustomerToERPMessage()
                {
                    FirstName = "John",
                    LastName = "Smith",
                    BillingAddress = new ShopifySendOrderAddressToERPMessage(),
                    DeliveryAddress = new ShopifySendOrderAddressToERPMessage(),
                }
            };

            CancellationToken cancellationToken;

            var loggerMock = autoMocker.GetMock<ILoggingAdapter>();
            var apiActorGroupMock = autoMocker.GetMock<IActorRefWrapper>();

            service.Init(apiActorGroupMock.Object, loggerMock.Object);


            // Act
            var returnMessage = await service.UpdateOrder(
                millenniumData,
                message,
                cancellationToken);

            // Assert
            Assert.Equal(Result.OK, returnMessage.Result);
        }



        private MillenniumService CreateService()
        {
            CreateMocks();

            return autoMocker.CreateInstance<MillenniumService>();
        }

        private void CreateMocks()
        {
            autoMocker.Use<TenantRepository>(x => x.Equals(It.IsAny<TenantRepository>()));
            autoMocker.GetMock<ITenantService>();
            autoMocker.GetMock<IMapper>();
            autoMocker.GetMock<IServiceProvider>();
            autoMocker.Use<MillenniumProductStockIntegrationRepository>(x => x.Equals(It.IsAny<MillenniumProductStockIntegrationRepository>()));
            autoMocker.Use<MillenniumNewStockProcessRepository>(x => x.Equals(It.IsAny<MillenniumNewStockProcessRepository>()));
            autoMocker.Use<MillenniumNewPricesProcessRepository>(x => x.Equals(It.IsAny<MillenniumNewPricesProcessRepository>()));
            autoMocker.Use<MillenniumProductPriceIntegrationRepository>(x => x.Equals(It.IsAny<MillenniumProductPriceIntegrationRepository>()));
            autoMocker.Use<MillenniumNewProductProcessRepository>(x => x.Equals(It.IsAny<MillenniumNewProductProcessRepository>()));
            autoMocker.Use<MillenniumProductIntegrationRepository>(x => x.Equals(It.IsAny<MillenniumProductIntegrationRepository>()));
            autoMocker.Use<MillenniumProductImageIntegrationRepository>(x => x.Equals(It.IsAny<MillenniumProductImageIntegrationRepository>()));
            autoMocker.Use<MillenniumImageIntegrationRepository>(x => x.Equals(It.IsAny<MillenniumImageIntegrationRepository>()));
            autoMocker.Use<MillenniumListProductManualProcessRepository>(x => x.Equals(It.IsAny<MillenniumListProductManualProcessRepository>()));
            autoMocker.Use<MillenniumUpdateOrderProcessRepository>(x => x.Equals(It.IsAny<MillenniumUpdateOrderProcessRepository>()));
            autoMocker.GetMock<ILoggingAdapter>();

            var methodPaymentRepositoryMock = autoMocker.GetMock<IMethodPaymentRepository>();
            var millenniumDomainServiceMock = autoMocker.GetMock<IMillenniumDomainService>();
            
            var millenniumUpdateOrderProcessRepositoryMock = autoMocker.GetMock<MillenniumUpdateOrderProcessRepository>();
            var apiActorGroupMock = autoMocker.GetMock<IActorRefWrapper>();

            Setup(methodPaymentRepositoryMock, 
                millenniumDomainServiceMock, 
                millenniumUpdateOrderProcessRepositoryMock,
                apiActorGroupMock);
        }

        private static void Setup(Mock<IMethodPaymentRepository> methodPaymentRepositoryMock,
            Mock<IMillenniumDomainService> millenniumDomainServiceMock,
            Mock<MillenniumUpdateOrderProcessRepository> millenniumUpdateOrderProcessRepositoryMock,
            Mock<IActorRefWrapper> apiActorGroupMock)
        {
            millenniumUpdateOrderProcessRepositoryMock.Setup(r => r.Save(It.IsAny<MillenniumUpdateOrderProcess>()));

            millenniumDomainServiceMock.Setup(m => m.ValidateNoteAtributtes(It.IsAny<MillenniumData>(), It.IsAny<ShopifySendOrderToERPMessage>()));
            millenniumDomainServiceMock.Setup(m => m.CalculateAdjusmentValue(It.IsAny<ShopifySendOrderToERPMessage>()))
                .Returns(0m);
            millenniumDomainServiceMock.Setup(m => m.IsMethodPaymentValid(It.IsAny<ShopifySendOrderToERPMessage>()))
                .Returns(true);
            millenniumDomainServiceMock.Setup(m => m.GetBandeira(It.IsAny<ShopifySendOrderToERPMessage>()))
                .Returns(MillenniumIssuerType.VISA);

            millenniumDomainServiceMock.Setup(m => m.GetLocation(It.IsAny<MillenniumData>(), It.IsAny<long?>()))
                .Returns(string.Empty);

            millenniumDomainServiceMock.Setup(m => m.GetNumeroParcelas(It.IsAny<ShopifySendOrderToERPMessage>(), It.IsAny<string>(), It.IsAny<MillenniumIssuerType>()))
                .Returns(1);

            millenniumDomainServiceMock.Setup(m => m.CalculateInitialValue(It.IsAny<ShopifySendOrderToERPMessage>(), It.IsAny<decimal>()))
                .Returns(1);

            millenniumDomainServiceMock.Setup(m => m.GetValuesWithFeesOrder(It.IsAny<decimal>(), It.IsAny<ShopifySendOrderToERPMessage>()))
                .Returns(1);

            methodPaymentRepositoryMock.Setup(r => r.GetMillenniumIssuerTypeAsync(It.IsAny<long>(), It.IsAny<ShopifySendOrderToERPMessage>()))
                .ReturnsAsync(string.Empty);



            apiActorGroupMock.Setup(a => a.Ask<ReturnMessage<MillenniumApiListOrdersResult>>
                (It.IsAny<MillenniumApiListOrdersRequest>(), It.IsAny<CancellationToken>()))
                             .ReturnsAsync(new ReturnMessage<MillenniumApiListOrdersResult>()
                             {
                                 Result = Result.OK,
                                 Data = new MillenniumApiListOrdersResult()
                                 {
                                     value = new List<MillenniumApiListOrdersResult.Value>()
                                 }
                             });


            apiActorGroupMock.Setup(a => a.Ask<ReturnMessage>
             (It.IsAny<MillenniumApiCreateOrderRequest>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ReturnMessage()
                          {
                              Result = Result.OK,
                          });
        }


    }
}
