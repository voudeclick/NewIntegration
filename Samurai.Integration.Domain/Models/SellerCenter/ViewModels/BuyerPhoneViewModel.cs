using Samurai.Integration.Domain.Models.SellerCenter.API.Orders.ValueObjects;
using System;
using System.Collections.Generic;

namespace Samurai.Integration.Domain.Models.SellerCenter.ViewModels
{
    public class BuyerPhoneViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public PersonType PersonType { get; set; }
        public DocumentType DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime? DocumentIssueDate { get; set; }
        public string DocumentIssuer { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; }
        public List<TranslationDto> Translations { get; set; }
        public List<BuyerFieldViewModel> CustomFields { get; set; }
        public List<BuyerAddressViewModel> Addresses { get; set; }
        public List<PhoneViewModel> Phones { get; set; }
        public List<object> CountriesList { get; set; }
        public bool CanEdit { get; set; }
    }
}
