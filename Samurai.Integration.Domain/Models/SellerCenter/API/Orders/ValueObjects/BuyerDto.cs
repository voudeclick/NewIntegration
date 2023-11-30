using Samurai.Integration.Domain.Models.SellerCenter.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects
{
    public class BuyerDto
    {

        public BuyerDto(){}

        public BuyerDto(OrderViewModel orderView)
        {
            Id = orderView.Buyer.Id;
            Addresses = orderView.Buyer.Addresses;
            ApprovalStatus = orderView.Buyer.ApprovalStatus;
            BirthDate = orderView.Buyer.BirthDate;
            CanEdit = orderView.Buyer.CanEdit;
            CountriesList = orderView.Buyer.CountriesList;
            CustomFields = orderView.Buyer.CustomFields;
            DocumentIssueDate = orderView.Buyer.DocumentIssueDate;
            DocumentIssuer = orderView.Buyer.DocumentIssuer;
            DocumentNumber = orderView.Buyer.DocumentNumber;
            DocumentType = orderView.Buyer.DocumentType;
            Email = orderView.Buyer.Email;
            FirstName = orderView.Buyer.FirstName;
            FullName = orderView.Buyer.FullName;
            LastName = orderView.Buyer.LastName;
            PersonType = orderView.Buyer.PersonType;
            Phones = orderView?.Buyer?.Phones?.Select(s => new BuyerPhoneDto(s)).ToList();
            Translations = orderView.Buyer.Translations;
        }

        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(256)]
        [Display(Name = "E-mail")]
        public string Email { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Nome")]
        public string FirstName { get; set; }

        [Display(Name = "Sobrenome")]
        [MaxLength(256)]
        public string LastName { get; set; }

        [Display(Name = "Nome")]
        [MaxLength(256)]
        public string FullName { get; set; }

        [Required]
        [Display(Name = "Data de nascimento")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? BirthDate { get; set; }

        [Required]
        [Display(Name = "Tipo de pessoa")]
        public PersonType PersonType { get; set; }

        [Required]
        [Display(Name = "Tipo")]
        public DocumentType DocumentType { get; set; }

        [Required]
        [MaxLength(256)]
        [Display(Name = "Número")]
        public string DocumentNumber { get; set; }

        [DataType(DataType.Date, ErrorMessage = "Formato inválido")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [Display(Name = "Data de expedição")]
        public DateTime? DocumentIssueDate { get; set; }

        [Display(Name = "Emissor")]
        public string DocumentIssuer { get; set; }

        [Display(Name = "Status de aprovação")]
        public ApprovalStatus ApprovalStatus { get; set; }

        [Display(Name = "Traduções")]

        public List<TranslationDto> Translations { get; set; }

        [Display(Name = "Campos customizados")]

        public List<BuyerFieldViewModel> CustomFields { get; set; }

        [Display(Name = "Endereços")]

        public List<BuyerAddressViewModel> Addresses { get; set; }

        [Display(Name = "Telefones")]

        public List<BuyerPhoneDto> Phones { get; set; }

        // Complementary Lists
        public List<object> CountriesList { get; set; }

        public bool CanEdit { get; set; }
    }

}
