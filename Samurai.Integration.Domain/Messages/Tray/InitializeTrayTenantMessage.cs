﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Messages.Tray
{
    public class InitializeTrayTenantMessage
    {
        public TenantDataMessage Data { get; set; }
    }
}
