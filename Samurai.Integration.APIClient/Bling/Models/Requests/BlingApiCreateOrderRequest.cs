using Samurai.Integration.Domain.Messages.SellerCenter.OrderActor;
using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Samurai.Integration.APIClient.Bling.Models.Requests
{
    [XmlRoot(ElementName = "pedido")]
    public class BlingApiCreateOrderRequest
    {
        [XmlElement(ElementName = "numero_loja")]
        public string NumeroLoja { get; set; }
        [XmlElement(ElementName = "cliente")]
        public Cliente Cliente { get; set; }
        [XmlElement(ElementName = "transporte")]
        public Transporte Transporte { get; set; }
        [XmlElement(ElementName = "itens")]
        public Itens Itens { get; set; }
        [XmlElement(ElementName = "parcelas")]
        public Parcelas Parcelas { get; set; }
        [XmlElement(ElementName = "vlr_frete")]
        public decimal Vlr_frete { get; set; }
        [XmlElement(ElementName = "vlr_desconto")]
        public string Vlr_desconto { get; set; }
        [XmlElement(ElementName = "obs")]
        public string Obs { get; set; }
        [XmlElement(ElementName = "obs_internas")]
        public string Obs_internas { get; set; }
        [XmlElement(ElementName = "idSituacao")]
        public int IdSituacao { get; set; }

        public static BlingApiCreateOrderRequest From(ProcessOrderMessage message, OrderSellerDto orderSeller, Domain.Messages.Bling.BlingData _blingData)
        {
            var delivery = orderSeller?.Deliveries?.FirstOrDefault(x => x.WarehouseId?.ToString() == message.SITenantWarehouseId);

            return new BlingApiCreateOrderRequest
            {
                NumeroLoja = $"{_blingData.OrderPrefix}{message.Number}",
                Cliente = Cliente.From(message),
                Transporte = Transporte.From(message, orderSeller),
                Itens = Itens.From(message, orderSeller),
                Parcelas = Parcelas.From(message),
                Vlr_frete = delivery.TotalPrice,
                Vlr_desconto = orderSeller.TotalDiscount.ToString(),
                Obs = orderSeller.Notes,
                IdSituacao = 9
            };
        }
    }

    [XmlRoot(ElementName = "cliente")]
    public class Cliente
    {
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [XmlElement(ElementName = "tipoPessoa")]
        public string TipoPessoa { get; set; }
        [XmlElement(ElementName = "endereco")]
        public string Endereco { get; set; }
        [XmlElement(ElementName = "cpf_cnpj")]
        public string Cpf_cnpj { get; set; }
        [XmlElement(ElementName = "ie")]
        public string Ie { get; set; }
        [XmlElement(ElementName = "numero")]
        public string Numero { get; set; }
        [XmlElement(ElementName = "complemento")]
        public string Complemento { get; set; }
        [XmlElement(ElementName = "bairro")]
        public string Bairro { get; set; }
        [XmlElement(ElementName = "cep")]
        public string Cep { get; set; }
        [XmlElement(ElementName = "cidade")]
        public string Cidade { get; set; }
        [XmlElement(ElementName = "uf")]
        public string Uf { get; set; }
        [XmlElement(ElementName = "fone")]
        public string Fone { get; set; }
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }

        public static Cliente From(ProcessOrderMessage message)
        {
            var address = message.Buyer?.Addresses?.FirstOrDefault();
            var phone = message.Buyer?.Phones?.FirstOrDefault();

            return new Cliente
            {
                Nome = message.Buyer?.FullName,
                TipoPessoa = message.Buyer?.DocumentType == DocumentType.CPF ? "F" : "J",
                Endereco = address?.Street,
                Cpf_cnpj = message.Buyer?.DocumentNumber?.Replace(".", string.Empty)?.Replace("-", string.Empty)?.Replace("/", string.Empty)?.Trim(),
                Numero = address?.Number,
                Complemento = address?.Complement,
                Bairro = address?.District,
                Cep = address?.ZipCode,
                Cidade = address?.City,
                Uf = address?.State,
                Fone = $"{phone?.AreaCode}{phone?.Number?.Replace("-", string.Empty)}",
                Email = message.Buyer?.Email
            };
        }
    }

    [XmlRoot(ElementName = "dados_etiqueta")]
    public class Dados_etiqueta
    {
        [XmlElement(ElementName = "nome")]
        public string Nome { get; set; }
        [XmlElement(ElementName = "endereco")]
        public string Endereco { get; set; }
        [XmlElement(ElementName = "numero")]
        public string Numero { get; set; }
        [XmlElement(ElementName = "complemento")]
        public string Complemento { get; set; }
        [XmlElement(ElementName = "municipio")]
        public string Municipio { get; set; }
        [XmlElement(ElementName = "uf")]
        public string Uf { get; set; }
        [XmlElement(ElementName = "cep")]
        public string Cep { get; set; }
        [XmlElement(ElementName = "bairro")]
        public string Bairro { get; set; }

        public static Dados_etiqueta From(string fullName, Address destinationAddres)
        {
            return new Dados_etiqueta
            {
                Nome = fullName,
                Endereco = destinationAddres?.Street,
                Numero = destinationAddres?.Number,
                Complemento = destinationAddres?.Complement,
                Municipio = destinationAddres?.City,
                Uf = destinationAddres?.State,
                Cep = destinationAddres?.ZipCode,
                Bairro = destinationAddres?.District
            };
        }
    }

    [XmlRoot(ElementName = "volume")]
    public class Volume
    {
        [XmlElement(ElementName = "servico")]
        public string Servico { get; set; }
        [XmlElement(ElementName = "codigoRastreamento")]
        public string CodigoRastreamento { get; set; }
    }

    [XmlRoot(ElementName = "volumes")]
    public class Volumes
    {
        [XmlElement(ElementName = "volume")]
        public List<Volume> Volume { get; set; }
    }

    [XmlRoot(ElementName = "transporte")]
    public class Transporte
    {
        [XmlElement(ElementName = "transportadora")]
        public string Transportadora { get; set; }
        [XmlElement(ElementName = "tipo_frete")]
        public string Tipo_frete { get; set; }
        [XmlElement(ElementName = "servico_correios")]
        public string Servico_correios { get; set; }
        [XmlElement(ElementName = "dados_etiqueta")]
        public Dados_etiqueta Dados_etiqueta { get; set; }
        [XmlElement(ElementName = "volumes")]
        public Volumes Volumes { get; set; }

        public static Transporte From(ProcessOrderMessage message, OrderSellerDto orderSeller)
        {
            var delivery = orderSeller?.Deliveries?.FirstOrDefault(x => x.WarehouseId?.ToString() == message.SITenantWarehouseId);

            return new Transporte
            {
                Transportadora = delivery?.ShipmentServiceName,
                Tipo_frete = "R",
                Dados_etiqueta = Dados_etiqueta.From(message.Buyer?.FullName, delivery?.DestinationAddress)
            };
        }
    }

    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "codigo")]
        public string Codigo { get; set; }
        [XmlElement(ElementName = "descricao")]
        public string Descricao { get; set; }
        [XmlElement(ElementName = "un")]
        public string Un { get; set; }
        [XmlElement(ElementName = "qtde")]
        public decimal Qtde { get; set; }
        [XmlElement(ElementName = "vlr_unit")]
        public decimal Vlr_unit { get; set; }

        public static Item From(OrderSellerDeliveryItemDto item)
        {
            return new Item
            {
                Codigo = item?.ProductClientCode,
                Descricao = item?.ProductName,
                Un = "un",
                Qtde = Convert.ToDecimal(item.Quantity),
                Vlr_unit = item.UnitPrice
            };
        }
    }

    [XmlRoot(ElementName = "itens")]
    public class Itens
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }

        public static Itens From(ProcessOrderMessage message, OrderSellerDto orderSeller)
        {
            var delivery = orderSeller?.Deliveries?.FirstOrDefault(x => x.WarehouseId?.ToString() == message.SITenantWarehouseId);

            return new Itens
            {
                Item = delivery?.Items?.Select(x => Requests.Item.From(x)).ToList()
            };
        }
    }
}

[XmlRoot(ElementName = "parcela")]
public class Parcela
{
    [XmlElement(ElementName = "data")]
    public string Data { get; set; }
    [XmlElement(ElementName = "vlr")]
    public decimal Vlr { get; set; }
    [XmlElement(ElementName = "obs")]
    public string Obs { get; set; }
}

[XmlRoot(ElementName = "parcelas")]
public class Parcelas
{
    [XmlElement(ElementName = "parcela")]
    public List<Parcela> Parcela { get; set; }

    public static Parcelas From(ProcessOrderMessage message)
    {
        var payment = message.Payments?.FirstOrDefault();

        return new Parcelas
        {
            Parcela = new List<Parcela> {
                new Parcela {
                     Vlr = payment.Value,
                     Obs = $"Forma de pagamento: {payment.PaymentMethod} | Parcelas: {payment.InstallmentCount}"
                }
            }
        };
    }
}

