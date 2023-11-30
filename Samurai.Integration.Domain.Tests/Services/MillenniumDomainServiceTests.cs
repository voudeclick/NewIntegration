using Moq;
using Samurai.Integration.Domain.Entities.Database;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Repositories;
using Samurai.Integration.Domain.Services;
using Samurai.Integration.Domain.Services.Interfaces;
using Samurai.Integration.EntityFramework.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Samurai.Integration.Domain.Tests.Services
{
    public class MillenniumDomainServiceTests
    {
        private readonly MockRepository mockRepository;


        public MillenniumDomainServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);            
        }

        [Fact]
        public void ValidateNoteAtributtes_IsNotValidTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var millenniumData = new Messages.Millennium.MillenniumData()
            {
                Id = 1,
            };

            var message = new ShopifySendOrderToERPMessage();

            // Act && Assert
            Assert.Throws<ArgumentException>(() =>
                service.ValidateNoteAtributtes(
                    millenniumData,
                    message));
        }

        [Fact]
        public void IsMethodPaymentValid_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var paymentTypes = new List<string> {
                        "DEPÓSITO BANCÁRIO",
                        "PIX",
                        "OUTROS",
                        "PAY2" };

            // Act && Assert
            foreach (var paymentType in paymentTypes)
            {

                var message = new ShopifySendOrderToERPMessage()
                {
                    PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                    {
                        PaymentType = paymentType,
                    }
                };
                
                var result = service.IsMethodPaymentValid(
                    message);

                Assert.True(result);
            }
        }

        [Fact]
        public void CalculateAdjusmentValue_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                InterestValue = 1,
                TaxValue = 1,
                DiscountsValues = 0.1m,
            };

            // Act
            var result = service.CalculateAdjusmentValue(
                message);

            // Assert
            Assert.Equal(1.9m, result);
        }

        [Fact]
        public void GetBandeira_IsNotValidTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
            };
            
            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                service.GetBandeira(message));
        }

        [Fact]
        public void GetBandeira_IsBoletoBancarioTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "boleto"
                }
            };

            // Act
            var millenniumIssuerType = service.GetBandeira(message);

            //Assert
            Assert.Equal(MillenniumIssuerType.BOLETO_BANCARIO, millenniumIssuerType);
            
        }

        [Fact]
        public void GetBandeira_IsPayPalTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "paypal"
                }
            };

            // Act
            var millenniumIssuerType = service.GetBandeira(message);

            //Assert
            Assert.Equal(MillenniumIssuerType.PAYPAL, millenniumIssuerType);
                        
        }


        [Fact]
        public void GetBandeira_IsOutrosTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var messages = new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "MANUAL", "TICKET", "VOUCHER", "CREDIT", "CREDITO", "DEBIT", "PIX", "OUTROS" }
            .Select(s => new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = s
                }
            });

            // Act && Assert
            foreach (var message in messages)
            {
                var millenniumIssuerType = service.GetBandeira(message);

                Assert.Equal(MillenniumIssuerType.OUTROS, millenniumIssuerType);
            }          
        }


        [Fact]
        public void GetBandeira_IssuerIsNullTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "issuer-is-null"
                }
            };

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                service.GetBandeira(message));

        }


        [Fact]
        public void GetBandeira_IssuerIsNotValidTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    Issuer = "is-not-valid"
                }
            };

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                service.GetBandeira(message));

        }

        [Fact]
        public void GetBandeira_IssuerIsValidTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    Issuer = "MASTERCARD"
                }
            };

            // Act
            var millenniumIssuerType = service.GetBandeira(message);

            //Assert
            Assert.Equal(MillenniumIssuerType.MASTERCARD, millenniumIssuerType);

        }



        [Fact]
        public void GetTipoPgto_IsNullTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            ShopifySendOrderToERPMessage message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                }
            };

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                service.GetTipoPgto(message));

        }

        [Fact]
        public void GetTipoPgto_IsBoletoTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "boleto"
                }
            };

            // Act
            var millenniumIssuerType = service.GetTipoPgto(message);

            //Assert
            Assert.Equal(MillenniumPaymentType.BOLETO, millenniumIssuerType);

        }

        [Fact]
        public void GetTipoPgto_IsCartaoTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "cartao"
                }
            };

            // Act
            var millenniumIssuerType = service.GetTipoPgto(message);

            //Assert
            Assert.Equal(MillenniumPaymentType.CARTAO, millenniumIssuerType);

        }


        [Fact]
        public void GetTipoPgto_IsOnlinePaymentsTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();


            var messages = new List<string> { "DEPÓSITO BANCÁRIO", "TRANSFERÊNCIA BANCÁRIA", "PIX", "DEPÓSITO", "Pay2" }
                            .Select(s =>
                                new ShopifySendOrderToERPMessage()
                                {
                                    PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                                    {
                                        PaymentType = s
                                    }
                                }
                                );

            //Act && Assert
            foreach (var message in messages)
            {
                var  millenniumPaymentType = service.GetTipoPgto(message);

                Assert.Equal(MillenniumPaymentType.ONLINEPAYMENTS, millenniumPaymentType);
            }

        }

        [Fact]
        public void GetTipoPgto_IsNotValidTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "cartao"
                }
            };
            // Act
            var millenniumIssuerType = service.GetTipoPgto(message);

            //Assert
            Assert.Equal(MillenniumPaymentType.CARTAO, millenniumIssuerType);
              
        }

        [Fact]
        public void GetTipoPgto_IsTryParseSuccessTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            ShopifySendOrderToERPMessage message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    PaymentType = "payment-not-valid"
                }
            };

            // Act && Assert
            Assert.Throws<ArgumentNullException>(() =>
                service.GetTipoPgto(message));

        }


        [Fact]
        public void CalculateInitialValue_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                Subtotal = 1,
                ShippingValue = 1,                
            };

            decimal adjustmentValue = 1;

            // Act
            var result = service.CalculateInitialValue(
                message,
                adjustmentValue);

            // Assert
            Assert.Equal(3,result);
        }

        [Fact]
        public void GetNumeroParcelas_IsPaypalTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {

            };

            string descricaoTipo = null;
            var bandeira = MillenniumIssuerType.PAYPAL;

            // Act
            var result = service.GetNumeroParcelas(
                message,
                descricaoTipo,
                bandeira);

            // Assert
            Assert.Equal(1,result);
        }

        [Fact]
        public void GetNumeroParcelas_IsInsidePaymentData_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();

            var message = new ShopifySendOrderToERPMessage()
            {
                PaymentData = new ShopifySendOrderPaymentDataToERPMessage()
                {
                    InstallmentQuantity = 10,
                }
            };

            string descricaoTipo = null;
            var bandeira = MillenniumIssuerType.MASTERCARD;

            // Act
            var result = service.GetNumeroParcelas(
                message,
                descricaoTipo,
                bandeira);

            // Assert
            Assert.Equal(10, result);
        }

        [Fact]
        public void GetLocation_HasMultiLocationTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var location = new LocationMap();
            location.SetLocations(new List<Models.LocationItem>()
            {
                new Models.LocationItem()
                {
                    ErpLocation = "000001",
                    EcommerceLocation = "36684955830"
                }
            });

            var millenniumData = new Messages.Millennium.MillenniumData()
            {
                HasMultiLocation = true,
                LocationMap = location
            };

            long? locationId = 36684955830;

            // Act
            var result = service.GetLocation(
                millenniumData,
                locationId);

            // Assert
            Assert.Equal("000001", result);
        }

        [Fact]
        public void GetLocation_HasMultiLocationExceptionTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var location = new LocationMap();
            location.SetLocations(new List<Models.LocationItem>()
            {
            });

            var millenniumData = new Messages.Millennium.MillenniumData()
            {
                HasMultiLocation = true,
                LocationMap = location
            };

            long? locationId = 36684955830;


            // Act && Assert
            Assert.Throws<Exception>(() =>
                service.GetLocation(
                millenniumData,
                locationId));

        }

        [Fact]
        public void GetValuesWithFeesOrder_ByValorInicialTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                NoteAttributes = new List<ShopifySendOrderNoteAttributeToERPMessage>()
            };

            // Act
            var result = service.GetValuesWithFeesOrder(10,message);

            // Assert
            Assert.Equal(10, result);

        }


        [Fact]
        public void GetValuesWithFeesOrder_ByNotesAttributesTest_ExpectedBehavior()
        {
            // Arrange
            var service = CreateService();
            var message = new ShopifySendOrderToERPMessage()
            {
                NoteAttributes = new List<ShopifySendOrderNoteAttributeToERPMessage>()
                {
                    new ShopifySendOrderNoteAttributeToERPMessage()
                    {
                        Name = "aditional_info_valor_total_pedido",
                        Value = "10.59"
                    }
                }
            };

            // Act
            var result = service.GetValuesWithFeesOrder(0, message);

            // Assert
            Assert.Equal(10.59m, result);

        }


        private IMillenniumDomainService CreateService()
        {
            var methodPaymentRepositoryMock = mockRepository.Create<IMethodPaymentRepository>();

            methodPaymentRepositoryMock.Setup(p => p.GetMillenniumIssuerTypeAsync(It.IsAny<long>(), It.IsAny<ShopifySendOrderToERPMessage>()))
                .ReturnsAsync("Teste");

            return new MillenniumDomainService(methodPaymentRepositoryMock.Object);
        }

    }
}
