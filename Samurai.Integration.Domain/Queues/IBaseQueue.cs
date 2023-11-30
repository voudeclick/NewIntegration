using System;
using System.Collections.Generic;
using System.Text;

namespace Samurai.Integration.Domain.Queues
{
    public interface IBaseQueue
    {
        long Id { get; set; }
        string StoreHandle { get; set; }
    }
}
