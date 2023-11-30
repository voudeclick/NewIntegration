using Samurai.Integration.Domain.Models.SellerCenter.API.Enums;
using System;
using System.ComponentModel.DataAnnotations;

using JadLogEnums = Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;


namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderSellerDeliveryPackageViewModel
    {
        public Guid Id { get; set; }

        [Display(Name = "Margem de lucro do Tenant")]
        public decimal? TenantMarkupAmount { get; set; }

        [Display(Name = "Margem de lucro do vendedor")]
        public decimal? SellerMarkupAmount { get; set; }

        [Display(Name = "Prazo de entrega")]
        public int? DeliveryTime { get; set; }

        [Display(Name = "Moeda")]
        public Currency Currency { get; set; }

        [Display(Name = "Valor")]
        public decimal? Price { get; set; }

        [Display(Name = "Código de rastreamento")]
        public string TrackingCode { get; set; }

        [Display(Name = "Status de postagem do pacote")]
        public TrackingPackagePostageStatus TrackingPackagePostageStatus { get; set; }

        [Display(Name = "ID do pacote de entrega de pedidos de rastreamento")]
        public Guid? TrackingOrderDeliveryPackageId { get; set; }

        [Display(Name = "ID da entrega do pedido de rastreamento")]
        public Guid? TrackingOrderDeliveryId { get; set; }

        [Display(Name = "ID do pedido JadLog")]
        public Guid? JadLogOrderId { get; set; }

        [Display(Name = "Código de pedido JadLog")]
        public string JadLogOrderCode { get; set; }

        [Display(Name = "Status do pedido JadLog")]
        public JadLogOrderStatus? JadLogOrderStatus { get; set; }

        [Display(Name = "Possui pedido JadLog")]
        public bool HasJadLogOrder { get { return JadLogOrderId.HasValue; } }

        [Display(Name = "Permite cancelamento do pedido JadLog Order")]
        public bool CanCancelJadLogOrder { get { return JadLogOrderId.HasValue && JadLogOrderStatus == JadLogEnums.JadLogOrderStatus.Included; } }

        [Display(Name = "Url de rastreamento")]
        public string TrackingUrl { get; set; }

        [Display(Name = "Data da nota fiscal")]
        public string InvoiceDate { get; set; }

        [Display(Name = "Número da nota fiscal")]
        public string InvoiceNumber { get; set; }

        [Display(Name = "Chave da nota fiscal")]
        public string InvoiceKey { get; set; }

        [Display(Name = "Link de consulta da nota fiscal")]
        public string InvoiceLink { get; set; }

        [Display(Name = "Id de entrega do cliente")]
        public string DeliveryClientId { get; set; }

    }
}