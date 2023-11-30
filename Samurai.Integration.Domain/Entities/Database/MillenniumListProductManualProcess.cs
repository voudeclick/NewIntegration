﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Entities.Database
{
    public class MillenniumListProductManualProcess
    {
		public Guid Id { get; set; }
		public long TenantId { get; set; }
		public string ProductId { get; set; }
		public DateTime ProcessDate { get; set; }
		public string MillenniumResult { get; set; }
		public string Exception { get; set; }
	}
}
