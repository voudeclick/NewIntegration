using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class OrderRefundBankDataDto
    {
        public Guid Id { get; set; }

        [Display(Name = "Código do banco")]
        public string BankNumber { get; set; }

        [Display(Name = "Agência")]
        public string AgencyNumber { get; set; }

        [Display(Name = "Dígito")]
        public string AgencyCheckNumber { get; set; }

        [Display(Name = "Conta")]
        public string AccountNumber { get; set; }

        [Display(Name = "Dígito")]
        public string AccountCheckNumber { get; set; }

        [Display(Name = "Nome")]
        public string HolderFullname { get; set; }

        [Display(Name = "Documento")]
        public string HolderDocumentNumber { get; set; }

        [Display(Name = "Tipo de documento")]
        public DocumentType HolderDocumentType { get; set; }
    }
}
