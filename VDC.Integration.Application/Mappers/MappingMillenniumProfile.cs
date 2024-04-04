using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using VDC.Integration.Application.Mappers.CustomResolvers;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Enums.Millennium;
using VDC.Integration.Domain.Messages.Shopify;
using VDC.Integration.Domain.Models.Millennium;

namespace VDC.Integration.Application.Mappers
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

            CreateMap<MillenniumData, Domain.Messages.Millennium.MillenniumData>();
        }
        class MilenniumApiCreateOrderPaymentDataRequestListConverter : ITypeConverter<ShopifySendOrderToERPMessage, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest>>
        {
            public IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> Convert(ShopifySendOrderToERPMessage source, IEnumerable<MilenniumApiCreateOrderPaymentDataRequest> destination, ResolutionContext context)
            {
                return new List<MilenniumApiCreateOrderPaymentDataRequest>() { context.Mapper.Map<MilenniumApiCreateOrderPaymentDataRequest>(source) };

            }
        }
    }
}
