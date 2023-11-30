using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Samurai.Integration.Domain.Messages.AliExpress.Order
{
    public class AliexpressOrderMessage
    {
        public string ExternalId { get; set; }
        public AliexpressAddressDropShipping LogisticsAddress { get; set; }
        public List<AliexpressOrderItem> Items { get; set; }

    }

    public class AliexpressAddressDropShipping
    {
        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ContactPerson { get; set; }
        public string Country { get => "BR"; }
        public string Cpf { get; set; }
        public string FullName { get; set; }
        public string Locale { get; set; }
        public string MobileNo { get; set; }
        public string PassportNo { get; set; }
        public string PassportNoDate { get; set; }
        public string PassportOrganization { get; set; }
        public string PhoneCountry { get => "+55"; } 
        public string Province { get; set; }
        public string TaxNumber { get; set; }
        public string Zip { get; set; }
        public string ForeignerPassportNo { get; set; }
        public string IsForeigner { get; set; }
        public string VatNo { get; set; }

    }
    public class AliexpressOrderItem
    {
        public long ProductCount { get; set; }
        public long ProductId { get; set; }
        public string SkuAttr { get; set; }
        public string LogisticsServiceName { get; set; }
        public string OrderMemo { get; set; }
    }

   

}
