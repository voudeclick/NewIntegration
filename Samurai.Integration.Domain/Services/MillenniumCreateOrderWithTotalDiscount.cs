using Samurai.Integration.Domain.Messages.Millennium;
using Samurai.Integration.Domain.Messages.Shopify;
using Samurai.Integration.Domain.Models.Millennium;
using Samurai.Integration.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Samurai.Integration.Domain.Services
{
    public class MillenniumCreateOrderWithTotalDiscount : ICreateOrder
    {
        readonly IMillenniumDomainService _millenniumDomainService;

        public MillenniumCreateOrderWithTotalDiscount(IMillenniumDomainService millenniumDomainService)
        {
            _millenniumDomainService = millenniumDomainService;
        }

        MillenniumApiCreateOrderRequest ICreateOrder.CreateOrder(MillenniumData millenniumData, ShopifySendOrderToERPMessage message, MilenniumApiCreateOrderPaymentDataRequest milenniumApiCreateOrderPaymentDataRequest, decimal adjustmentValue)
        {
            var itens = new List<MilenniumApiCreateOrderProductRequest>();
            for (int i = 0; i < message.Items.Count; i++)
            {
                var item = message.Items[i];
                itens.Add(new MilenniumApiCreateOrderProductRequest
                {
                    cod_filial = _millenniumDomainService.GetLocation(millenniumData, item.LocationId),
                    quantidade = item.Quantity,
                    preco = item.Price,
                    sku = item.Sku,
                    encomenda = millenniumData.GetFlagEncomenda(),
                    obs_item = item.GetFlagGift(),
                    item = (i + 1).ToString(),
                });
            }

            var createOrder = new MillenniumApiCreateOrderRequest
            {
                //TODO -> PENDENTE DEFINICAO MILLENIUM
                //cod_filial = GetLocation(millenniumData,message.Items.FirstOrDefault()?.LocationId?.ToString()),
                cod_pedidov = message.ExternalID,
                data_emissao = message.CreatedAt.ToString("yyyy-MM-dd"),
                data_entrega = message.GetDataEntregaToString(),
                total = message.Subtotal,
                v_acerto = adjustmentValue == 0 ? (decimal?)null : adjustmentValue,
                acerto = adjustmentValue == 0 ? (decimal?)null : adjustmentValue / message.Subtotal * 100,
                aprovado = message.Approved,
                n_pedido_cliente = message.ID.ToString(),
                quantidade = message.Items.Sum(i => i.Quantity),
                v_frete = message.ShippingValue,
                origem_pedido = "SITE",
                vitrine = millenniumData.VitrineId,
                obs = message.vendor,
                nome_transportadora = message.CarrierName,
                mensagens = new List<MilenniumApiCreateOrderMessage> { new MilenniumApiCreateOrderMessage { texto = message.Note } },
                produtos = itens,
                dados_cliente = new List<MilenniumApiCreateOrderCustomerRequest>
                            {
                            new MilenniumApiCreateOrderCustomerRequest
                            {
                                nome = string.Concat(message.Customer.FirstName, " ", message.Customer.LastName),
                                data_aniversario = message.Customer.BirthDate.HasValue ? message.Customer.BirthDate.Value.ToString("yyyy-MM-dd") : null,
                                ddd_cel = message.Customer.DDD,
                                cel = message.Customer.Phone,
                                e_mail = message.Customer.Email,
                                obs = message.Customer.Note,
                                vitrine = millenniumData.VitrineId,
                                cpf = !millenniumData.DisableCustomerDocument ? message.Customer.Company : string.Empty,
                                ie = message.NoteAttributes.FirstOrDefault(n => n.Name == "aditional_info_extra_billing_IE")?.Value ?? null,
                                ufie = message.NoteAttributes.FirstOrDefault(n => n.Name == "aditional_info_extra_billing_UFIE")?.Value ?? null,
                                endereco = new List<MilenniumApiCreateOrderAddressRequest>
                                {
                                    new MilenniumApiCreateOrderAddressRequest
                                    {
                                        bairro = message.Customer.BillingAddress.District,
                                        cep = message.Customer.BillingAddress.ZipCode,
                                        cidade = message.Customer.BillingAddress.City,
                                        complemento = message.Customer.BillingAddress.Complement,
                                        contato = message.Customer.BillingAddress.Contact,
                                        estado = message.Customer.BillingAddress.State,
                                        ddd = message.Customer.BillingAddress.DDD,
                                        fone = message.Customer.BillingAddress.Phone,
                                        logradouro = message.Customer.BillingAddress.Address,
                                        numero = message.Customer.BillingAddress.Number
                                    }
                                },
                                endereco_entrega = new List<MilenniumApiCreateOrderAddressRequest>
                                {
                                    new MilenniumApiCreateOrderAddressRequest
                                    {
                                        bairro = message.Customer.DeliveryAddress.District,
                                        cep = message.Customer.DeliveryAddress.ZipCode,
                                        cidade = message.Customer.DeliveryAddress.City,
                                        complemento = message.Customer.DeliveryAddress.Complement,
                                        contato = message.Customer.DeliveryAddress.Contact,
                                        estado = message.Customer.DeliveryAddress.State,
                                        ddd = message.Customer.DeliveryAddress.DDD,
                                        fone = message.Customer.DeliveryAddress.Phone,
                                        logradouro = message.Customer.DeliveryAddress.Address,
                                        numero = message.Customer.DeliveryAddress.Number
                                    }
                                }
                            }
                            },
                lancamentos = new List<MilenniumApiCreateOrderPaymentDataRequest>
                        {
                            milenniumApiCreateOrderPaymentDataRequest
                        }

            };
            return createOrder;
        }

        bool ICreateOrder.TypeDiscount(MillenniumData millenniumData) => !millenniumData.EnableProductDiscount;
    }
}
