﻿using System;

namespace VDC.Integration.Domain.Models
{
    public class IntegrationCache
    {
        public string Id { get; set; }
        public byte[] Value { get; set; }
        public DateTimeOffset ExpiresAtTime { get; set; }
        public long? SlidingExpirationInSeconds { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
