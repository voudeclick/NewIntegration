using AutoMapper;
using Samurai.Integration.Application.Mappers.CustomResolvers;
using Samurai.Integration.Domain.Entities.Database.TenantData;
using Samurai.Integration.Domain.Enums.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Millennium;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.Application.Mappers
{
    public class MappingMillenniumProfile : Profile
    {
        public MappingMillenniumProfile()
        {
            CreateMap<ShopifySendOrderToERPMessage, MilenniumApiCreateOrderPaymentDataRequest>()
               .ForMember(dest => dest.valor_inicial, map => map.MapFrom(src => src.Subtotal + src.ShippingValue + src.AdjustmentValue))
               .ForMember(dest => dest.bandeira, map => map.MapFrom<MillenniumIssuerTypeResolver>())
               .ForMember(dest => dest.numparc, map => map.MapFrom((src, dest) => (MillenniumIssuerType)dest.bandeira == MillenniumIssuerType.PAYPAL ? 1 : src.PaymentData.InstallmentQuantity))
               .ForMember(dest => dest.parcela, map => map.MapFrom((src, dest) => (MillenniumIssuerType)dest.bandeira == MillenniumIssuerType.PAYPAL ? 1 : src.PaymentData.InstallmentQuantity))
               .ForMember(dest => dest.operadora, map => map.MapFrom(src => (int)src.OperatorType));

            CreateMap<ShopifySendOrderToERPMessage, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest>>()
                  .ConvertUsing<MilenniumApiCreateOrderPaymentDataRequestListConverter>();

            CreateMap<ShopifySendOrderToERPMessage, MillenniumApiCreateOrderRequest>()
                .AfterMap((src, dest, context) =>
                {
                    dest.lancamentos = context.Mapper.Map<ShopifySendOrderToERPMessage, List<MilenniumApiCreateOrderPaymentDataRequest>>(src);
                })
                .ForMember(dest => dest.cod_pedidov, map => map.MapFrom(src => src.ExternalID))
                .ForMember(dest => dest.data_emissao, map => map.MapFrom(src => src.CreatedAt.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.data_entrega, map => map.MapFrom(src => src.CreatedAt.AddDays(src.DaysToDelivery).ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.total, map => map.MapFrom(src => src.Subtotal))
                .ForMember(dest => dest.v_acerto, map => map.MapFrom(src => src.AdjustmentValue == 0 ? (decimal?)null : src.AdjustmentValue))
                .ForMember(dest => dest.acerto, map => map.MapFrom(src => src.AdjustmentValue == 0 ? (decimal?)null : src.AdjustmentValue / src.Subtotal * 100))
                .ForMember(dest => dest.aprovado, map => map.MapFrom(src => src.Approved))
                .ForMember(dest => dest.n_pedido_cliente, map => map.MapFrom(src => src.ID))
                .ForMember(dest => dest.quantidade, map => map.MapFrom(src => src.Items.Sum(i => i.Quantity)))
                .ForMember(dest => dest.v_frete, map => map.MapFrom(src => src.ShippingValue))
                .ForMember(dest => dest.origem_pedido, map => map.MapFrom(src => "SITE"))
                .ForMember(dest => dest.vitrine, map => map.MapFrom(src => src.VitrineId))
                .ForMember(dest => dest.nome_transportadora, map => map.MapFrom(src => src.CarrierName))
                .ForMember(dest => dest.mensagens, map => map.MapFrom(src => new List<MilenniumApiCreateOrderMessage> { new MilenniumApiCreateOrderMessage { texto = src.Note } }))
                .ForMember(dest => dest.produtos, map => map.MapFrom(src => src.Items.Select(i => new MilenniumApiCreateOrderProductRequest
                {
                    quantidade = i.Quantity,
                    preco = i.Price,
                    sku = i.Sku
                }).ToList()))
                .ForMember(dest => dest.dados_cliente, map => map.MapFrom(src => new List<MilenniumApiCreateOrderCustomerRequest>
                    {
                        new MilenniumApiCreateOrderCustomerRequest
                        {
                            nome = string.Concat(src.Customer.FirstName, " ", src.Customer.LastName),
                            ddd_cel = src.Customer.DDD,
                            cel = src.Customer.Phone,
                            e_mail = src.Customer.Email,
                            obs = src.Customer.Note,
                            vitrine = src.VitrineId,
                            cpf = !src.DisableCustomerDocument ? src.Customer.Company : string.Empty,
                            ie = src.GetNoteAttributeByName("aditional_info_extra_billing_IE"),
                            ufie = src.GetNoteAttributeByName("aditional_info_extra_billing_UFIE"),
                            endereco = new List<MilenniumApiCreateOrderAddressRequest>
                            {
                                new  MilenniumApiCreateOrderAddressRequest
                                {
                                bairro = src.Customer.BillingAddress.District,
                                cep = src.Customer.BillingAddress.ZipCode,
                                cidade = src.Customer.BillingAddress.City,
                                complemento = src.Customer.BillingAddress.Complement,
                                contato = src.Customer.BillingAddress.Contact,
                                estado = src.Customer.BillingAddress.State,
                                ddd = src.Customer.BillingAddress.DDD,
                                fone = src.Customer.BillingAddress.Phone,
                                logradouro = src.Customer.BillingAddress.Address,
                                numero = src.Customer.BillingAddress.Number
                                }
                            },
                            endereco_entrega = new List<MilenniumApiCreateOrderAddressRequest>
                            {
                                new  MilenniumApiCreateOrderAddressRequest
                                {
                                bairro = src.Customer.DeliveryAddress.District,
                                cep = src.Customer.DeliveryAddress.ZipCode,
                                cidade = src.Customer.DeliveryAddress.City,
                                complemento = src.Customer.DeliveryAddress.Complement,
                                contato = src.Customer.DeliveryAddress.Contact,
                                estado = src.Customer.DeliveryAddress.State,
                                ddd = src.Customer.DeliveryAddress.DDD,
                                fone = src.Customer.DeliveryAddress.Phone,
                                logradouro = src.Customer.DeliveryAddress.Address,
                                numero = src.Customer.DeliveryAddress.Number
                                 }
                            }
                        }
                    })
                );


            CreateMap<Domain.Models.SellerCenter.API.Orders.Order, MilenniumApiCreateOrderPaymentDataRequest>()
           .ForMember(dest => dest.valor_inicial, map => map.MapFrom(src => src.SubTotal + src.AdjustmentValue)) //Todo -> validar
           .ForMember(dest => dest.bandeira, map => map.MapFrom<SellerCenterPaymentTypeResolver>())
           .ForMember(dest => dest.numparc, map => map.MapFrom((src, dest) => (MillenniumIssuerType)dest.bandeira == MillenniumIssuerType.PAYPAL ? 1 : src.Payments.FirstOrDefault().InstallmentCount))
           .ForMember(dest => dest.parcela, map => map.MapFrom((src, dest) => (MillenniumIssuerType)dest.bandeira == MillenniumIssuerType.PAYPAL ? 1 : src.Payments.FirstOrDefault().InstallmentCount))
           .ForMember(dest => dest.operadora, map => map.MapFrom(src => (int)src.OperatorType)); // Todo -> tem que incluir operationTYpe na msg

            CreateMap<Domain.Models.SellerCenter.API.Orders.Order, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest>>()
                  .ConvertUsing<SelleCenterMilenniumApiListConverter>();
            
            CreateMap<MillenniumData, Domain.Messages.Millennium.MillenniumData>();

            CreateMap<Domain.Models.SellerCenter.API.Orders.Order, MillenniumApiCreateOrderRequest>()
                .AfterMap((src, dest, context) =>
                {
                    dest.lancamentos = context.Mapper.Map<Domain.Models.SellerCenter.API.Orders.Order, List<MilenniumApiCreateOrderPaymentDataRequest>>(src);
                })
                .ForMember(dest => dest.cod_pedidov, map => map.MapFrom(src => src.Number))
                .ForMember(dest => dest.data_emissao, map => map.MapFrom(src => src.CreateDate.ToString("yyyy-MM-dd")))
                .ForMember(dest => dest.data_entrega, map => map.MapFrom(src => src.CreateDate.AddDays(1).ToString("yyyy-MM-dd"))) //Todo -> Validar
                .ForMember(dest => dest.total, map => map.MapFrom(src => src.SubTotal))
                .ForMember(dest => dest.v_acerto, map => map.MapFrom(src => src.AdjustmentValue == 0 ? (decimal?)null : src.AdjustmentValue))
                .ForMember(dest => dest.acerto, map => map.MapFrom(src => src.AdjustmentValue == 0 ? (decimal?)null : src.AdjustmentValue / src.SubTotal * 100))
                .ForMember(dest => dest.aprovado, map => map.MapFrom(src => src.Buyer.ApprovalStatus == Domain.Models.SellerCenter.API.Orders.ValueObjects.ApprovalStatus.Approved))
                .ForMember(dest => dest.n_pedido_cliente, map => map.MapFrom(src => src.Id))
                .ForMember(dest => dest.quantidade, map => map.MapFrom(src => src.OrderSellers.SelectMany(x => x.Deliveries).Select(i => i.Items.Count()).FirstOrDefault()))
                .ForMember(dest => dest.v_frete, map => map.MapFrom(src => 0)) //Todo -> Validar
                .ForMember(dest => dest.origem_pedido, map => map.MapFrom(src => "SITE"))
                .ForMember(dest => dest.vitrine, map => map.MapFrom(src => src.VitrineId))
                //.ForMember(dest => dest.nome_transportadora, map => map.MapFrom(src => src.CarrierName))
                .ForMember(dest => dest.mensagens, map => map.MapFrom(src => new List<MilenniumApiCreateOrderMessage> { new MilenniumApiCreateOrderMessage { texto = "" } }))
                .ForMember(dest => dest.produtos, map => map.MapFrom(src => src.OrderSellers.SelectMany(x => x.Deliveries.SelectMany(x => x.Items)).Select(i => new MilenniumApiCreateOrderProductRequest
                {
                    quantidade = i.Quantity,
                    preco = i.FinalPrice,
                    sku = i.Sku
                }).ToList()))
                .ForMember(dest => dest.dados_cliente, map => map.MapFrom(src => new List<MilenniumApiCreateOrderCustomerRequest>
                    {
                        new MilenniumApiCreateOrderCustomerRequest
                        {
                            nome = string.Concat(src.Buyer.FirstName, " ", src.Buyer.LastName),
                            ddd_cel = src.Buyer.Phones.FirstOrDefault().AreaCode,
                            cel = src.Buyer.Phones.FirstOrDefault().Number,
                            e_mail = src.Buyer.Email,
                            obs = "",
                            vitrine = src.VitrineId,
                            cpf = !src.DisableCustomerDocument ? src.Buyer.DocumentNumber : string.Empty, //Todo -> Validar
                            //ie = src.GetNoteAttributeByName("aditional_info_extra_billing_IE"),
                            //ufie = src.GetNoteAttributeByName("aditional_info_extra_billing_UFIE"),
                            endereco = new List<MilenniumApiCreateOrderAddressRequest>
                            {
                                new  MilenniumApiCreateOrderAddressRequest
                                {
                                bairro = src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.District,
                                cep =  src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.ZipCode,
                                cidade =  src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.City,
                                complemento =  src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.Complement,
                                contato =  src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.Number,
                                estado =  src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.State,
                                ddd =  "",
                                fone = "",
                                logradouro = src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.Street,
                                numero = src.OrderSellers.FirstOrDefault().Deliveries.FirstOrDefault().OriginAddress.Number
                                 }
                            },
                            endereco_entrega = new List<MilenniumApiCreateOrderAddressRequest>
                            {
                                new  MilenniumApiCreateOrderAddressRequest
                                {
                                bairro = src.Buyer.Addresses.FirstOrDefault().District,
                                cep =  src.Buyer.Addresses.FirstOrDefault().ZipCode,
                                cidade =  src.Buyer.Addresses.FirstOrDefault().City,
                                complemento =  src.Buyer.Addresses.FirstOrDefault().Complement,
                                contato =  src.Buyer.Phones.FirstOrDefault().Number,
                                estado =  src.Buyer.Addresses.FirstOrDefault().State,
                                ddd =  src.Buyer.Phones.FirstOrDefault().AreaCode,
                                fone = src.Buyer.Phones.FirstOrDefault().Number,
                                logradouro = src.Buyer.Addresses.FirstOrDefault().Street,
                                numero = src.Buyer.Addresses.FirstOrDefault().Number
                                 }
                            }
                        }
                    })
                );


        }
        class MilenniumApiCreateOrderPaymentDataRequestListConverter : ITypeConverter<ShopifySendOrderToERPMessage, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest>>
        {
            public IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> Convert(ShopifySendOrderToERPMessage source, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> destination, ResolutionContext context)
            {
                return new List<MilenniumApiCreateOrderPaymentDataRequest>() { context.Mapper.Map<MilenniumApiCreateOrderPaymentDataRequest>(source) };

            }
        }
        class SelleCenterMilenniumApiListConverter : ITypeConverter<Domain.Models.SellerCenter.API.Orders.Order, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest>>
        {
            public IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> Convert(Domain.Models.SellerCenter.API.Orders.Order source, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> destination, ResolutionContext context)
            {
                return new List<MilenniumApiCreateOrderPaymentDataRequest>() { context.Mapper.Map<MilenniumApiCreateOrderPaymentDataRequest>(source) };

            }
        }

    }
}
