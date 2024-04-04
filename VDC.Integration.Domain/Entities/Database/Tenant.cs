using System;
using System.Collections.Generic;
using VDC.Integration.Domain.Entities.Database.TenantData;
using VDC.Integration.Domain.Enums;
using VDC.Integration.Domain.Queues;

namespace VDC.Integration.Domain.Entities.Database
{
    public class Tenant : BaseEntity, IBaseQueue
    {
        public bool Status { get; set; }
        /// <summary>
        /// Integracao de Produtos
        /// </summary>
        public bool ProductIntegrationStatus { get; set; }
        [Obsolete]
        public bool SetProductsAsUnpublished { get; set; }
        [Obsolete]
        public BodyIntegrationType BodyIntegrationType { get; set; }
        [Obsolete]
        public bool ProductGroupingEnabled { get; set; }
        [Obsolete]
        public bool ProductDescriptionIsHTML { get; set; }
        [Obsolete]
        public bool WriteCategoryNameTags { get; set; }
        public bool ImageIntegrationEnabled { get; set; }
        /// <summary>
        /// Integracao de pedidos
        /// </summary>
        public bool OrderIntegrationStatus { get; set; }
        [Obsolete]
        public bool DisableCustomerDocument { get; set; }
        [Obsolete]
        public bool DisableAddressParse { get; set; }
        [Obsolete]
        public bool ParsePhoneDDD { get; set; }
        public string StoreName { get; set; }
        public string StoreHandle { get; set; }
        [Obsolete]
        public string ShopifyStoreDomain { get; set; }
        [Obsolete]
        public string ShopifyAppJson { get; set; }

        public IntegrationType IntegrationType { get; set; }

        public TenantType Type { get; set; }


        [Obsolete]
        public int DaysToDelivery { get; set; }
        [Obsolete]
        public long MinOrderId { get; set; }
        public bool MultiLocation { get; set; }
        public bool TenantLogging { get; set; }
        public DateTime? DeactivatedDate { get; set; }
        public string Search { get; set; }
        //Integration Type
        public virtual ShopifyData ShopifyData { get; set; }

        //Tenant Type
        public virtual MillenniumData MillenniumData { get; set; }
        public virtual OmieData OmieData { get; set; }
        public virtual LocationMap LocationMap { get; set; }
        public virtual List<MethodPayment.MethodPayment> MethodPayments { get; set; }

        public Tenant HideSensitiveData()
        {
            ShopifyData?.HideSensitiveData();
            MillenniumData?.HideSensitiveData();
            OmieData?.HideSensitiveData();
            return this;
        }

        public void BeforeSaving()
        {
            Search = $"{Id} {StoreName} {Type} {IntegrationType}";

            if (!Status && !DeactivatedDate.HasValue)
            {
                DeactivatedDate = DateTime.Now;
            }

            if (Status && DeactivatedDate.HasValue)
            {
                DeactivatedDate = null;
            }
        }
    }
}
