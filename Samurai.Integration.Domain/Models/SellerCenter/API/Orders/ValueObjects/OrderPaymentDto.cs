using System;
using System.ComponentModel.DataAnnotations;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderPaymentDto
    {
        public Guid Id { get; set; }

        [Display(Name = "Forma de pagamento")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Gateway de pagamento")]
        public string PaymentGatewayName { get; set; }
        public PaymentGatewayType PaymentGatewayType { get; set; }

        [Display(Name = "Status")]
        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "Total")]
        public decimal Value { get; set; }

        public string CreditCardHash { get; set; }

        public string CreditCardHolderFullname { get; set; }

        public string CreditCardHolderBirthDate { get; set; }

        public DocumentType? CreditCardHolderDocumentType { get; set; }

        public string CreditCardHolderDocumentNumber { get; set; }

        public PhoneType? CreditCardHolderPhoneType { get; set; }

        public string CreditCardHolderPhoneCountryCode { get; set; }

        public string CreditCardHolderPhoneAreaCode { get; set; }

        public string CreditCardHolderPhoneNumber { get; set; }

        public string CreditCardHolderPhoneExtension { get; set; }

        [Display(Name = "Boleto")]
        public string BoletoReference { get; set; }

        [Display(Name = "Parcelas")]
        public int InstallmentCount { get; set; }
    }

}
