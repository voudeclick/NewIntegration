﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.AliExpress
{
    public class InitializeAliExpressTenantMessage
    {
        public TenantDataMessage Data { get; set; }
    }
}
