﻿using Samurai.Integration.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database.Integrations.Millenium
{
    public class MillenniumProductImageIntegration
    {
		public Guid Id { get; set; }
		public long TenantId { get; set; }
		public long IdProduto { get; set; }
		public string ExternalId { get; set; }
		public string Payload { get; set; }
		public IntegrationStatus Status { get; set; }
		public DateTime IntegrationDate { get; set; }
		public Guid? MillenniumListProductProcessId { get; set; }
		public Guid? MillenniumIntegrationProductReferenceId { get; set; }
		public string Routine { get; set; }
	}
}
