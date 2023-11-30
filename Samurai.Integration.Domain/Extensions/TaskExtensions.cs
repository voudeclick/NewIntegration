using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Samurai.Integration.Domain.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<Task> CloseAsyncSafe(this Task task) 
        {
            if (task != null) await task;
            task = null;
            return task;
        }
    }
}
