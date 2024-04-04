using System.Threading.Tasks;

namespace VDC.Integration.Domain.Extensions
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
